using System;

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

        public PositionalPropertiesUsageAnalyzer(MessageTemplateParser messageTemplateParser, CodeAnnotationsCache codeAnnotationsCache)
        {
            _messageTemplateParser = messageTemplateParser;
            _templateParameterNameAttributeProvider = codeAnnotationsCache.GetLazyProvider<TemplateParameterNameAttributeProvider>();
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

            foreach (var property in messageTemplate.PositionalProperties)
            {
                consumer.AddHighlighting(new PositionalPropertyUsedWarning(templateExpression.GetTokenInformation(property)));
            }
        }
    }
}
