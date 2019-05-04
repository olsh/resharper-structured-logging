using System.Linq;

using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Util;

using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.Analyzer
{
    [ElementProblemAnalyzer(typeof(IInvocationExpression))]
    public class CorrectExceptionPassingAnalyzer : ElementProblemAnalyzer<IInvocationExpression>
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

            var templateArgumentIndex = templateArgument.IndexOf();
            var exceptionType = element.PsiModule.GetPredefinedType().TryGetType(PredefinedType.EXCEPTION_FQN, NullableAnnotation.Unknown);
            if (exceptionType == null)
            {
                return;
            }

            ICSharpArgument invalidExceptionArgument = null;
            foreach (var argument in element.ArgumentList.Arguments)
            {
                var argumentType = argument.Value?.Type();
                if (!(argumentType is IDeclaredType declaredType))
                {
                    continue;
                }

                if (!declaredType.IsSubtypeOf(exceptionType))
                {
                    continue;
                }

                if (templateArgumentIndex > argument.IndexOf())
                {
                    return;
                }

                invalidExceptionArgument = argument;
                break;
            }

            if (invalidExceptionArgument == null)
            {
                return;
            }

            var overloadAvailable = false;
            var candidates = element.InvocationExpressionReference.GetCandidates().ToArray();
            var invalidArgumentIndex = invalidExceptionArgument.IndexOf();
            foreach (var candidate in candidates)
            {
                if (!(candidate.GetDeclaredElement() is IMethod declaredElement))
                {
                    continue;
                }

                foreach (var parameter in declaredElement.Parameters)
                {
                    if (invalidArgumentIndex <= parameter.IndexOf())
                    {
                        break;
                    }

                    if (parameter.Type.IsSubtypeOf(exceptionType))
                    {
                        overloadAvailable = true;
                        break;
                    }
                }

                if (overloadAvailable)
                {
                    break;
                }
            }

            if (!overloadAvailable)
            {
                return;
            }

            consumer.AddHighlighting(new ExceptionPassedAsTemplateArgumentWarning(invalidExceptionArgument.GetDocumentRange()));
        }
    }
}