using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;

using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.Analyzer
{
    [ElementProblemAnalyzer(
        typeof(IInvocationExpression),
        HighlightingTypes = new[] { typeof(ContextualLoggerWarning) })]
    public class ContextualLoggerFactoryAnalyzer : ElementProblemAnalyzer<IInvocationExpression>
    {
        protected override void Run(
            IInvocationExpression element,
            ElementProblemAnalyzerData data,
            IHighlightingConsumer consumer)
        {
            if (!element.IsContextualLoggerFactoryMethod())
            {
                return;
            }

            var containingNode = element.GetContainingNode<ITypeDeclaration>();
            if (containingNode == null)
            {
                return;
            }

            var contextType = element.TypeArguments[0];

            // A generic wrapper such as ILogger<T> Create<T>(ILoggerFactory factory) => factory.CreateLogger<T>()
            // names its own type parameter, so the caller of the wrapper decides the context, not this call.
            if (contextType == null || contextType.IsTypeParameterType())
            {
                return;
            }

            if (contextType.GetScalarType()
                    ?.GetClrName()
                    .FullName == containingNode.CLRName)
            {
                return;
            }

            // A composition root builds loggers for the types it wires up, so its own type would be
            // the wrong context there. Only a logger that stays behind is this type's logger.
            if (!element.IsOwnLoggerOfContainingType(containingNode))
            {
                return;
            }

            consumer.AddHighlighting(
                new ContextualLoggerWarning(
                    element.GetDocumentRange(),
                    element.GetFirstTypeArgumentNode(),
                    containingNode.DeclaredElement));
        }
    }
}
