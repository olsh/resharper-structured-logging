using System.Linq;

using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeStyle.Suggestions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
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

        [CanBeNull]
        public static IStringLiteralAlterer GetMessageTemplateStringLiteral(this IInvocationExpression invocation)
        {
            var templateArgument = invocation.GetTemplateArgument();
            if (templateArgument == null)
            {
                return null;
            }

            return StringLiteralAltererUtil.TryCreateStringLiteralByExpression(templateArgument.Value);
        }

        public static DocumentRange GetTokenDocumentRange(this DocumentRange documentRange, MessageTemplateToken token)
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
            if (className == "Microsoft.Extensions.Logging.LoggerExtensions" || className == "NLog.Logger")
            {
                return "message";
            }

            return null;
        }
    }
}
