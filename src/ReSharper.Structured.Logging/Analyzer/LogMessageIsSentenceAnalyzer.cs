using System.Text.RegularExpressions;

using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Util;

using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.Analyzer
{
    [ElementProblemAnalyzer(typeof(IInvocationExpression))]
    public class LogMessageIsSentenceAnalyzer : ElementProblemAnalyzer<IInvocationExpression>
    {
        private static readonly Regex DotAtTheEnd = new Regex(@"\.+$", RegexOptions.Compiled);

        protected override void Run(IInvocationExpression element, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            var templateArgument = element.GetTemplateArgument();
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

            consumer.AddHighlighting(new LogMessageIsSentenceWarning(lastFragmentExpression));
        }
    }
}
