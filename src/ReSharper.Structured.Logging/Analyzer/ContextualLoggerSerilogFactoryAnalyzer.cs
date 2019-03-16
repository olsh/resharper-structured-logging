using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.Analyzer
{
    [ElementProblemAnalyzer(typeof(IInvocationExpression))]
    public class ContextualLoggerSerilogFactoryAnalyzer : ElementProblemAnalyzer<IInvocationExpression>
    {
        protected override void Run(IInvocationExpression invocation, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            if (!invocation.IsSerilogContextFactoryLogger())
            {
                return;
            }

            var containingNode = invocation.GetContainingNode<ITypeDeclaration>();
            if (containingNode == null)
            {
                return;
            }

            var invocationTypeArgument = invocation.TypeArguments[0]?.GetScalarType()?.GetClrName();
            if (invocationTypeArgument?.FullName == containingNode.CLRName)
            {
                return;
            }

            consumer.AddHighlighting(new DuplicateTemplatePropertyWarning(invocation.GetDocumentRange()));
        }
    }
}
