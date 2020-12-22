using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Impl.Resolve;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Util;

using ReSharper.Structured.Logging.Models;
using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.Extensions
{
    public static class PsiExtensions
    {
        [CanBeNull]
        public static ICSharpArgument GetTemplateArgument(this IInvocationExpression invocationExpression)
        {
            if (!(invocationExpression.Reference.Resolve().DeclaredElement is ITypeMember typeMember))
            {
                return null;
            }

            var templateParameterName = typeMember.GetTemplateParameterName();
            if (string.IsNullOrEmpty(templateParameterName))
            {
                return null;
            }

            return invocationExpression.ArgumentList.Arguments.FirstOrDefault(
                a => a.MatchingParameter?.Element.ShortName == templateParameterName);
        }

        public static MessageTemplateTokenInformation GetTokenInformation(this ICSharpArgument argument, MessageTemplateToken token)
        {
            var (tokenTextRange, tokenArgument) = FindTokenTextRange(argument, token);
            var tokenDocument = argument.GetDocumentRange().Document;
            var documentRange = new DocumentRange(tokenDocument, tokenTextRange);

            return new MessageTemplateTokenInformation(documentRange, tokenArgument);
        }

        private static (TextRange, IStringLiteralAlterer) FindTokenTextRange(this ICSharpArgument argument, MessageTemplateToken token)
        {
            var documentRange = argument.GetDocumentRange();
            if (argument.Value is IAdditiveExpression additiveExpression && additiveExpression.ConstantValue.IsString())
            {
                var arguments = new LinkedList<ExpressionArgumentInfo>();
                FlattenAdditiveExpression(additiveExpression, arguments);

                var globalOffset = 0;
                foreach (var additiveArgument in arguments)
                {
                    var range = additiveArgument.GetDocumentRange();
                    var start = range.StartOffset.Offset;
                    var end = range.EndOffset.Offset;

                    // The token index is zero-based and we remove two quotes
                    if (token.StartIndex < end - start - 3 + globalOffset)
                    {
                        var tokenStartIndex = start + token.StartIndex - globalOffset + 1;
                        var tokenEndIndex = tokenStartIndex + token.Length;

                        return (new TextRange(tokenStartIndex, end > tokenEndIndex ? tokenEndIndex : end), StringLiteralAltererUtil.TryCreateStringLiteralByExpression(additiveArgument.Expression));
                    }

                    globalOffset += end - start - 2;
                }
            }

            var startOffset = documentRange.TextRange.StartOffset + token.StartIndex + 1;

            // ReSharper disable once AssignNullToNotNullAttribute
            return (new TextRange(startOffset, startOffset + token.Length), StringLiteralAltererUtil.TryCreateStringLiteralByExpression(argument.Expression));
        }

        public static string TryGetTemplateText(this ICSharpArgument argument)
        {
            if (argument.Value is IAdditiveExpression additiveExpression && additiveExpression.ConstantValue.IsString())
            {
                var linkedList = new LinkedList<ExpressionArgumentInfo>();
                FlattenAdditiveExpression(additiveExpression, linkedList);

                return string.Join(string.Empty, linkedList.Select(l => StringLiteralAltererUtil
                    .TryCreateStringLiteralByExpression(l.Expression)
                    ?.Expression.GetUnquotedText()));
            }

            return StringLiteralAltererUtil.TryCreateStringLiteralByExpression(argument.Value)?.Expression.GetUnquotedText();
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

            var declaredElement = invocationExpression.Reference.Resolve().DeclaredElement as IClrDeclaredElement;
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

        private static void FlattenAdditiveExpression(IAdditiveExpression additiveExpression, LinkedList<ExpressionArgumentInfo> list)
        {
            foreach (var argumentInfo in additiveExpression.Arguments)
            {
                if (argumentInfo is ExpressionArgumentInfo expressionArgumentInfo && expressionArgumentInfo.Expression is IAdditiveExpression additive)
                {
                    FlattenAdditiveExpression(additive, list);

                    continue;
                }

                list.AddLast((ExpressionArgumentInfo)argumentInfo);
            }
        }

        private static string GetTemplateParameterName([NotNull] this ITypeMember typeMember)
        {
            var templateFormatAttribute = typeMember.GetAttributeInstances(AttributesSource.All)
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
                return typeMember.ShortName == "BeginScope" ? "messageFormat" : "message";
            }

            return null;
        }
    }
}
