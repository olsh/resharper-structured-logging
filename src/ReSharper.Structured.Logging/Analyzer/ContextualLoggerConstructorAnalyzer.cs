using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;

using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.Analyzer
{
    [ElementProblemAnalyzer(typeof(IConstructorDeclaration), typeof(IPrimaryConstructorDeclaration))]
    public class ContextualLoggerConstructorAnalyzer : ElementProblemAnalyzer<ICSharpParametersOwnerDeclaration>
    {
        // ReSharper disable once CognitiveComplexity
        protected override void Run(
            ICSharpParametersOwnerDeclaration element,
            ElementProblemAnalyzerData data,
            IHighlightingConsumer consumer)
        {
            var containingType = element.DeclaredElement?.GetContainingType();
            var className = containingType?.GetClrName()
                .FullName;
            if (className == null)
            {
                return;
            }

            foreach (var declaration in element.ParameterDeclarations)
            {
                if (!(declaration.Type is IDeclaredType declaredType))
                {
                    continue;
                }

                if (!declaredType.IsGenericMicrosoftExtensionsLogger())
                {
                    continue;
                }

                var argumentType = declaredType.GetFirstGenericArgumentType();
                if (argumentType == null)
                {
                    continue;
                }

                if (className.Equals(
                        argumentType.GetClassType()
                            ?.GetClrName()
                            .FullName))
                {
                    continue;
                }

                consumer.AddHighlighting(
                    new ContextualLoggerWarning(
                        declaration.TypeUsage.GetDocumentRange(),
                        declaration.TypeUsage.GetFirstTypeArgumentNode(),
                        containingType,
                        declaration));
            }
        }
    }
}
