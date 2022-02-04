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
    public class DuplicatePropertiesTemplateAnalyzer : ElementProblemAnalyzer<IInvocationExpression>
    {
        private readonly MessageTemplateParser _messageTemplateParser;

        private readonly TemplateParameterNameAttributeProvider _templateParameterNameAttributeProvider;

        public DuplicatePropertiesTemplateAnalyzer(MessageTemplateParser messageTemplateParser, CodeAnnotationsCache codeAnnotationsCache)
        {
            _messageTemplateParser = messageTemplateParser;
            _templateParameterNameAttributeProvider = codeAnnotationsCache.GetProvider<TemplateParameterNameAttributeProvider>();
        }

        protected override void Run(
            IInvocationExpression element,
            ElementProblemAnalyzerData data,
            IHighlightingConsumer consumer)
        {
            var templateArgument = element.GetTemplateArgument(_templateParameterNameAttributeProvider);
            var templateText = templateArgument?.TryGetTemplateText();
            if (templateText == null)
            {
                return;
            }

            var messageTemplate = _messageTemplateParser.Parse(templateText);
            if (messageTemplate.NamedProperties == null)
            {
                return;
            }

            foreach (var duplicates in messageTemplate.NamedProperties
                .GroupBy(n => n.PropertyName)
                .Where(g => g.Count() > 1))
            {
                foreach (var token in duplicates)
                {
                    consumer.AddHighlighting(new DuplicateTemplatePropertyWarning(templateArgument.GetTokenInformation(token)));
                }
            }
        }
    }
}
