using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.Analyzer
{
    [ElementProblemAnalyzer(typeof(IInvocationExpression))]
    public class CompileTimeConstantTemplateAnalyzer : ElementProblemAnalyzer<IInvocationExpression>
    {
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

            if (templateArgument.Value.IsConstantValue())
            {
                return;
            }

            consumer.AddHighlighting(new TemplateIsNotCompileTimeConstantWarning(templateArgument.Expression.GetDocumentRange()));
        }
    }
}
