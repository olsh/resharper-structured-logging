using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;

using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.Analyzer
{
    [ElementProblemAnalyzer(typeof(IConstructorDeclaration))]
    public class ContextualLoggerConstructorAnalyzer : ElementProblemAnalyzer<IConstructorDeclaration>
    {
        // ReSharper disable once CognitiveComplexity
        protected override void Run(IConstructorDeclaration element, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            if (element.Params?.ParameterDeclarations == null)
            {
                return;
            }

            foreach (var declaration in element.Params.ParameterDeclarations)
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

                var containingType = element.DeclaredElement?.GetContainingType();
                var className = containingType?.GetClrName().FullName;
                if (className == null)
                {
                    continue;
                }

                if (className.Equals(argumentType.GetClassType()?.GetClrName().FullName))
                {
                    continue;
                }

                consumer.AddHighlighting(new ContextualLoggerWarning(declaration.TypeUsage.GetDocumentRange()));
            }
        }
    }
}
