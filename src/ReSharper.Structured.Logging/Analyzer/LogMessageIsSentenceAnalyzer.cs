using System;
using System.Text.RegularExpressions;

using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Util;

using ReSharper.Structured.Logging.Caching;
using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.Analyzer
{
    [ElementProblemAnalyzer(typeof(IInvocationExpression), HighlightingTypes = new[] { typeof(LogMessageIsSentenceWarning) })]
    public class LogMessageIsSentenceAnalyzer : ElementProblemAnalyzer<IInvocationExpression>
    {
        private readonly Lazy<TemplateParameterNameAttributeProvider> _templateParameterNameAttributeProvider;

        private static readonly Regex DotAtTheEnd = new Regex(@"(?<!\.)\.$", RegexOptions.Compiled);

        public LogMessageIsSentenceAnalyzer(CodeAnnotationsCache codeAnnotationsCache)
        {
            _templateParameterNameAttributeProvider = codeAnnotationsCache.GetLazyProvider<TemplateParameterNameAttributeProvider>();
        }

        protected override void Run(IInvocationExpression element, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            var templateArgument = element.GetTemplateArgument(_templateParameterNameAttributeProvider.Value);
            var lastFragmentExpression = templateArgument?.TryCreateLastTemplateFragmentExpression();
            if (lastFragmentExpression == null)
            {
                return;
            }

            var unquotedText = lastFragmentExpression.Expression.GetUnquotedText();
            if (!DotAtTheEnd.IsMatch(unquotedText))
            {
                return;
            }

            consumer.AddHighlighting(new LogMessageIsSentenceWarning(lastFragmentExpression, DotAtTheEnd));
        }
    }
}
