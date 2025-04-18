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
    [ElementProblemAnalyzer(typeof(IInvocationExpression))]
    public class PositionalPropertiesUsageAnalyzer : ElementProblemAnalyzer<IInvocationExpression>
    {
        private readonly MessageTemplateParser _messageTemplateParser;

        private readonly Lazy<TemplateParameterNameAttributeProvider> _templateParameterNameAttributeProvider;

        public PositionalPropertiesUsageAnalyzer(MessageTemplateParser messageTemplateParser, CodeAnnotationsCache codeAnnotationsCache)
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
            var templateText = templateArgument?.TryGetTemplateText();
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
                consumer.AddHighlighting(new PositionalPropertyUsedWarning(templateArgument.GetTokenInformation(property)));
            }
        }
    }
}
