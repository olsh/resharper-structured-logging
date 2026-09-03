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
    [ElementProblemAnalyzer(typeof(IInvocationExpression), typeof(IAttribute))]
    public class PositionalPropertiesUsageAnalyzer : ElementProblemAnalyzer<ICSharpArgumentsOwner>
    {
        private readonly MessageTemplateParser _messageTemplateParser;

        private readonly Lazy<TemplateParameterNameAttributeProvider> _templateParameterNameAttributeProvider;

        public PositionalPropertiesUsageAnalyzer(
            MessageTemplateParser messageTemplateParser,
            CodeAnnotationsCache codeAnnotationsCache)
        {
            _messageTemplateParser = messageTemplateParser;
            _templateParameterNameAttributeProvider =
                codeAnnotationsCache.GetLazyProvider<TemplateParameterNameAttributeProvider>();
        }

        protected override void Run(
            ICSharpArgumentsOwner element,
            ElementProblemAnalyzerData data,
            IHighlightingConsumer consumer)
        {
            var templateExpression = element.GetTemplateExpression(_templateParameterNameAttributeProvider.Value);
            var templateText = templateExpression?.TryGetTemplateText();
            if (templateText == null)
            {
                return;
            }

            var messageTemplate = _messageTemplateParser.Parse(templateText);
            if (messageTemplate.PositionalProperties == null)
            {
                return;
            }

            var holeArguments = element.GetTemplateHoleArguments(_templateParameterNameAttributeProvider.Value);
            var usedPropertyNames = messageTemplate.PositionalProperties.Select(p => p.PropertyName)
                .ToArray();

            foreach (var property in messageTemplate.PositionalProperties)
            {
                // A positional hole names the argument it takes, so {1} is filled by the second value
                var argument = holeArguments != null
                               && property.TryGetPositionalValue(out var position)
                               && position < holeArguments.Count
                    ? holeArguments[position]
                    : null;

                consumer.AddHighlighting(
                    new PositionalPropertyUsedWarning(
                        templateExpression.GetTokenInformation(property),
                        property,
                        argument,
                        usedPropertyNames));
            }
        }
    }
}
