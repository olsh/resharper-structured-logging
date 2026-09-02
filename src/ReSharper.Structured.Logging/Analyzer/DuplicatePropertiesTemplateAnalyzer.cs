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
    public class DuplicatePropertiesTemplateAnalyzer : ElementProblemAnalyzer<ICSharpArgumentsOwner>
    {
        private readonly MessageTemplateParser _messageTemplateParser;

        private readonly Lazy<TemplateParameterNameAttributeProvider> _templateParameterNameAttributeProvider;

        public DuplicatePropertiesTemplateAnalyzer(
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
            if (messageTemplate.NamedProperties == null)
            {
                return;
            }

            var holeArguments = element.GetTemplateHoleArguments(_templateParameterNameAttributeProvider.Value);
            var usedPropertyNames = messageTemplate.NamedProperties.Select(n => n.PropertyName)
                .ToArray();

            foreach (var duplicates in messageTemplate.NamedProperties
                         .GroupBy(n => n.PropertyName)
                         .Where(g => g.Count() > 1))
            {
                foreach (var token in duplicates)
                {
                    var holeIndex = Array.IndexOf(messageTemplate.NamedProperties, token);
                    var argument = holeArguments != null && holeIndex >= 0 && holeIndex < holeArguments.Count
                        ? holeArguments[holeIndex]
                        : null;

                    consumer.AddHighlighting(
                        new DuplicateTemplatePropertyWarning(
                            templateExpression.GetTokenInformation(token),
                            token,
                            argument,
                            usedPropertyNames));
                }
            }
        }
    }
}
