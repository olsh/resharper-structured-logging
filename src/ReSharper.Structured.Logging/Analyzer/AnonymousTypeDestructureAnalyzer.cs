using System.Linq;

using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Util;

using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Highlighting;
using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.Analyzer
{
    [ElementProblemAnalyzer(typeof(IInvocationExpression))]
    public class AnonymousTypeDestructureAnalyzer : ElementProblemAnalyzer<IInvocationExpression>
    {
        private readonly MessageTemplateParser _messageTemplateParser;

        public AnonymousTypeDestructureAnalyzer(MessageTemplateParser messageTemplateParser)
        {
            _messageTemplateParser = messageTemplateParser;
        }

        protected override void Run(
            IInvocationExpression element,
            ElementProblemAnalyzerData data,
            IHighlightingConsumer consumer)
        {
            var templateArgument = element.GetTemplateArgument();
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

            var stringLiteral = StringLiteralAltererUtil.TryCreateStringLiteralByExpression(templateArgument.Value);
            if (stringLiteral == null)
            {
                return;
            }

            var messageTemplate = _messageTemplateParser.Parse(stringLiteral.Expression.GetUnquotedText());
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

                    consumer.AddHighlighting(new AnonymousObjectDestructuringWarning(stringLiteral, namedProperty, stringLiteral.GetTokenDocumentRange(namedProperty)));
                }
            }
        }
    }
}