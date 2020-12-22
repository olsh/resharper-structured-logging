using System.Linq;

using JetBrains.ReSharper.Daemon.StringAnalysis;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon.Attributes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Highlighting;
using ReSharper.Structured.Logging.Serilog.Events;
using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.Analyzer
{
    [ElementProblemAnalyzer(typeof(IInvocationExpression))]
    public class TemplateFormatAnalyzer : ElementProblemAnalyzer<IInvocationExpression>
    {
        private readonly MessageTemplateParser _messageTemplateParser;

        public TemplateFormatAnalyzer(MessageTemplateParser messageTemplateParser)
        {
            _messageTemplateParser = messageTemplateParser;
        }

        protected override void Run(
            IInvocationExpression element,
            ElementProblemAnalyzerData data,
            IHighlightingConsumer consumer)
        {
            var templateArgument = element.GetTemplateArgument();
            var templateText = templateArgument?.TryGetTemplateText();
            if (templateText == null)
            {
                return;
            }

            var messageTemplate = _messageTemplateParser.Parse(templateText);

            HighlightTemplate(consumer, templateArgument, messageTemplate);
            HighlightUnusedArguments(element, consumer, templateArgument, messageTemplate);

            var argumentsCount = element.ArgumentList.Arguments.Count - templateArgument.IndexOf() - 1;
            if (messageTemplate.NamedProperties != null)
            {
                foreach (var property in messageTemplate.NamedProperties)
                {
                    argumentsCount--;

                    if (argumentsCount < 0)
                    {
                        consumer.AddHighlighting(
                            new TemplateFormatStringUnexistingArgumentWarning(
                                templateArgument.GetTokenInformation(property).DocumentRange));
                    }
                }
            }
            else if (messageTemplate.PositionalProperties != null)
            {
                foreach (var property in messageTemplate.PositionalProperties)
                {
                    if (!property.TryGetPositionalValue(out int position))
                    {
                        continue;
                    }

                    if (position >= argumentsCount)
                    {
                        consumer.AddHighlighting(
                            new TemplateFormatStringUnexistingArgumentWarning(
                                templateArgument.GetTokenInformation(property).DocumentRange));
                    }
                }
            }
        }

        private static void HighlightTemplate(
            IHighlightingConsumer consumer,
            ICSharpArgument templateArgument,
            MessageTemplate messageTemplate)
        {
            foreach (var token in messageTemplate.Tokens)
            {
                if (!(token is PropertyToken))
                {
                    continue;
                }

                consumer.AddHighlighting(
                    new StringEscapeCharacterHighlighting(
                        templateArgument.GetTokenInformation(token).DocumentRange,
                        DefaultLanguageAttributeIds.FORMAT_STRING_ITEM));
            }
        }

        private static void HighlightUnusedArguments(
            IInvocationExpression element,
            IHighlightingConsumer consumer,
            ICSharpArgument templateArgument,
            MessageTemplate messageTemplate)
        {
            var templateArgumentIndex = templateArgument.IndexOf();
            foreach (var argument in element.ArgumentList.Arguments)
            {
                var argumentIndex = argument.IndexOf();
                if (argumentIndex <= templateArgumentIndex)
                {
                    continue;
                }

                var argumentPosition = argumentIndex - templateArgumentIndex;
                if (messageTemplate.NamedProperties != null)
                {
                    if (messageTemplate.NamedProperties.Length < argumentPosition)
                    {
                        consumer.AddHighlighting(new TemplateFormatStringArgumentIsNotUsedWarning(argument));
                    }
                }
                else if (messageTemplate.PositionalProperties != null)
                {
                    if (!messageTemplate.PositionalProperties.Any(
                            p => p.TryGetPositionalValue(out int position) && position == argumentPosition - 1))
                    {
                        consumer.AddHighlighting(new TemplateFormatStringArgumentIsNotUsedWarning(argument));
                    }
                }
                else
                {
                    consumer.AddHighlighting(new TemplateFormatStringArgumentIsNotUsedWarning(argument));
                }
            }
        }
    }
}
