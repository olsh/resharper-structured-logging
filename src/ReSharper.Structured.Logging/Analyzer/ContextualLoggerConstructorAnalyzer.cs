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
        protected override void Run(IConstructorDeclaration constructor, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            if (constructor.Params?.ParameterDeclarations == null)
            {
                return;
            }

            foreach (var declaration in constructor.Params.ParameterDeclarations)
            {
                if (!(declaration.Type is IDeclaredType declaredType))
                {
                    return;
                }

                if (!declaredType.IsGenericMicrosoftExtensionsLogger())
                {
                    return;
                }

                var argumentType = declaredType.GetFirstGenericArgumentType();
                if (argumentType == null)
                {
                    return;
                }

                var containingType = constructor.DeclaredElement?.GetContainingType();
                var className = containingType?.GetClrName().FullName;
                if (className == null)
                {
                    return;
                }

                if (className.Equals(argumentType.GetClassType()?.GetClrName().FullName))
                {
                    return;
                }

                consumer.AddHighlighting(new ContextualLoggerWarning(declaration.TypeUsage.GetDocumentRange()));
            }
        }
    }
}
