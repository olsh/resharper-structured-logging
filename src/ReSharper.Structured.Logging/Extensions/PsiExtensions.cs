using System.Linq;

using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeStyle.Suggestions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Util;

using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.Extensions
{
    public static class PsiExtensions
    {
        [CanBeNull]
        public static ICSharpArgument GetTemplateArgument(this IInvocationExpression invocationExpression)
        {
            if (!(invocationExpression.Reference?.Resolve()
                      .DeclaredElement is ITypeMember typeMember))
            {
                return null;
            }

            var templateParameterName = typeMember.GetTemplateParameterName();
            if (string.IsNullOrEmpty(templateParameterName))
            {
                return null;
            }

            return invocationExpression.ArgumentList.Arguments.FirstOrDefault(
                a => a.GetMatchingParameterName() == templateParameterName);
        }

        public static DocumentRange GetTokenDocumentRange(this ICSharpArgument argument, MessageTemplateToken token)
        {
            var documentRange = argument.GetDocumentRange();

            return GetTokenDocumentRange(token, documentRange);
        }

        public static DocumentRange GetTokenDocumentRange(this IStringLiteralAlterer stringLiteralAlterer, MessageTemplateToken token)
        {
            var documentRange = stringLiteralAlterer.Expression.GetDocumentRange();

            return GetTokenDocumentRange(token, documentRange);
        }

        public static bool IsGenericMicrosoftExtensionsLogger([NotNull]this IDeclaredType declared)
        {
            return declared.GetClrName().FullName == "Microsoft.Extensions.Logging.ILogger`1";
        }

        public static bool IsSerilogContextFactoryLogger([NotNull]this IInvocationExpression invocationExpression)
        {
            if (invocationExpression.TypeArguments.Count != 1)
            {
                return false;
            }

            var declaredElement = invocationExpression.Reference?.Resolve().DeclaredElement as IClrDeclaredElement;
            var containingType = declaredElement?.GetContainingType();
            if (containingType == null)
            {
                return false;
            }

            if (containingType.GetClrName().FullName == "Serilog.ILogger" && declaredElement.ShortName == "ForContext")
            {
                return true;
            }

            return false;
        }

        [CanBeNull]
        public static IType GetFirstGenericArgumentType([NotNull]this IDeclaredType declared)
        {
            var substitution = declared.GetSubstitution();
            var typeParameter = substitution.Domain.FirstOrDefault();
            if (typeParameter == null)
            {
                return null;
            }

            return substitution.Apply(typeParameter);
        }

        private static DocumentRange GetTokenDocumentRange(
            MessageTemplateToken token,
            DocumentRange documentRange)
        {
            var startOffset = documentRange.TextRange.StartOffset + token.StartIndex + 1;

            return new DocumentRange(documentRange.Document, new TextRange(startOffset, startOffset + token.Length));
        }

        private static string GetTemplateParameterName([NotNull] this ITypeMember typeMember)
        {
            var templateFormatAttribute = typeMember.GetAttributeInstances(true)
                .FirstOrDefault(a => a.GetAttributeShortName() == "MessageTemplateFormatMethodAttribute");

            if (templateFormatAttribute != null)
            {
                return templateFormatAttribute.PositionParameters()
                    .FirstOrDefault()
                    ?.ConstantValue.Value?.ToString();
            }

            var className = typeMember.GetContainingType()?.GetClrName().FullName;
            if (className == "Microsoft.Extensions.Logging.LoggerExtensions")
            {
                return "message";
            }

            return null;
        }
    }
}
