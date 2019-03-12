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

        private static DocumentRange GetTokenDocumentRange(
            MessageTemplateToken token,
            DocumentRange documentRange)
        {
            var startOffset = documentRange.TextRange.StartOffset + token.StartIndex + 1;

            return new DocumentRange(documentRange.Document, new TextRange(startOffset, startOffset + token.Length));
        }

        private static string GetTemplateParameterName(this ITypeMember typeMember)
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
