using System;
using System.Linq;

using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;

using ReSharper.Structured.Logging.Caching;
using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Highlighting;
using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.Analyzer
{
    [ElementProblemAnalyzer(typeof(IInvocationExpression))]
    public class AnonymousTypeDestructureAnalyzer : ElementProblemAnalyzer<IInvocationExpression>
    {
        private readonly MessageTemplateParser _messageTemplateParser;

        private readonly Lazy<TemplateParameterNameAttributeProvider> _templateParameterNameAttributeProvider;

        public AnonymousTypeDestructureAnalyzer(
            MessageTemplateParser messageTemplateParser,
            CodeAnnotationsCache codeAnnotationsCache)
        {
            _messageTemplateParser = messageTemplateParser;
            _templateParameterNameAttributeProvider =
                codeAnnotationsCache.GetLazyProvider<TemplateParameterNameAttributeProvider>();
        }

        protected override void Run(
            IInvocationExpression element,
            ElementProblemAnalyzerData data,
            IHighlightingConsumer consumer)
        {
            var templateArgument = element.GetTemplateArgument(_templateParameterNameAttributeProvider.Value);
            if (templateArgument == null)
            {
                return;
            }

            // Template holes of LoggerMessage.Define/DefineScope are filled by generic type parameters,
            // so the arguments that follow the template are not hole values
            if (element.IsLoggerMessageDefineMethod())
            {
                return;
            }

            var holeArguments = element.GetTemplateHoleArguments(templateArgument);
            if (holeArguments == null || !holeArguments.Any(a => a.Value is IAnonymousObjectCreationExpression))
            {
                return;
            }

            var templateText = templateArgument.Value.TryGetTemplateText();
            if (templateText == null)
            {
                return;
            }

            var messageTemplate = _messageTemplateParser.Parse(templateText);
            if (messageTemplate.NamedProperties == null)
            {
                return;
            }

            var holeCount = Math.Min(holeArguments.Count, messageTemplate.NamedProperties.Length);
            for (var index = 0; index < holeCount; index++)
            {
                if (!(holeArguments[index].Value is IAnonymousObjectCreationExpression))
                {
                    continue;
                }

                var namedProperty = messageTemplate.NamedProperties[index];
                if (namedProperty.Destructuring != Destructuring.Default)
                {
                    continue;
                }

                var tokenInformation = templateArgument.Value.GetTokenInformation(namedProperty);
                consumer.AddHighlighting(new AnonymousObjectDestructuringWarning(tokenInformation));
            }
        }
    }
}
