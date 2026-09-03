using System;
using System.Text.RegularExpressions;

using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;

using ReSharper.Structured.Logging.Caching;
using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.Analyzer
{
    [ElementProblemAnalyzer(typeof(IInvocationExpression), typeof(IAttribute), HighlightingTypes = new[] { typeof(LogMessageIsSentenceWarning) })]
    public class LogMessageIsSentenceAnalyzer : ElementProblemAnalyzer<ICSharpArgumentsOwner>
    {
        private readonly Lazy<TemplateParameterNameAttributeProvider> _templateParameterNameAttributeProvider;

        private static readonly Regex DotAtTheEnd = new Regex(@"(?<!\.)\.$", RegexOptions.Compiled);

        public LogMessageIsSentenceAnalyzer(CodeAnnotationsCache codeAnnotationsCache)
        {
            _templateParameterNameAttributeProvider = codeAnnotationsCache.GetLazyProvider<TemplateParameterNameAttributeProvider>();
        }

        protected override void Run(ICSharpArgumentsOwner element, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            var templateExpression = element.GetTemplateExpression(_templateParameterNameAttributeProvider.Value);
            if (templateExpression == null)
            {
                return;
            }

            // A ZLogger 2.x template is the interpolated string itself, so the text the message ends with is
            // whatever precedes the closing quotes. There is no literal for the fix to rewrite
            if (templateExpression.IsZLoggerTemplateHandler())
            {
                if (DotAtTheEnd.IsMatch(templateExpression.GetText()
                        .TrimEnd('"')))
                {
                    consumer.AddHighlighting(
                        new LogMessageIsSentenceWarning(templateExpression.GetDocumentRange(), DotAtTheEnd));
                }

                return;
            }

            var lastFragmentExpression = templateExpression.TryCreateLastTemplateFragmentExpression();
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
