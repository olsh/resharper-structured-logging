using System;
using System.Linq;

using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
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

        public AnonymousTypeDestructureAnalyzer(MessageTemplateParser messageTemplateParser, CodeAnnotationsCache codeAnnotationsCache)
        {
            _messageTemplateParser = messageTemplateParser;
            _templateParameterNameAttributeProvider = codeAnnotationsCache.GetLazyProvider<TemplateParameterNameAttributeProvider>();
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

            var anonymousObjectsArguments = element.ArgumentList.Arguments
                .Where(a => a.Value is IAnonymousObjectCreationExpression)
                .ToArray();
            if (anonymousObjectsArguments.Length == 0)
            {
                return;
            }

            var templateText = templateArgument.TryGetTemplateText();
            if (templateText == null)
            {
                return;
            }

            var messageTemplate = _messageTemplateParser.Parse(templateText);
            if (messageTemplate.NamedProperties == null)
            {
                return;
            }

            var templateArgumentIndex = templateArgument.IndexOf();
            foreach (var argument in anonymousObjectsArguments)
            {
                var index = argument.IndexOf() - templateArgumentIndex - 1;
                if (index < messageTemplate.NamedProperties.Length)
                {
                    var namedProperty = messageTemplate.NamedProperties[index];
                    if (namedProperty.Destructuring != Destructuring.Default)
                    {
                        continue;
                    }

                    var tokenInformation = templateArgument.GetTokenInformation(namedProperty);
                    consumer.AddHighlighting(new AnonymousObjectDestructuringWarning(tokenInformation));
                }
            }
        }
    }
}
