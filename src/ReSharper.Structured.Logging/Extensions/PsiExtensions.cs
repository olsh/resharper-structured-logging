using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Impl.Resolve;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Util;

using ReSharper.Structured.Logging.Caching;
using ReSharper.Structured.Logging.Models;
using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.Extensions
{
    public static class PsiExtensions
    {
        private static readonly IClrTypeName LogContextFqn = new ClrTypeName("Serilog.Context.LogContext");

        [CanBeNull]
        public static ICSharpArgument GetTemplateArgument(this IInvocationExpression invocationExpression, TemplateParameterNameAttributeProvider templateParameterNameAttributeProvider)
        {
            if (!(invocationExpression.Reference.Resolve().DeclaredElement is ITypeMember typeMember))
            {
                return null;
            }

            var templateParameterName = templateParameterNameAttributeProvider.GetInfo(typeMember);
            if (string.IsNullOrEmpty(templateParameterName))
            {
                return null;
            }

            foreach (var argument in invocationExpression.ArgumentList.Arguments)
            {
                if (argument.MatchingParameter?.Element.ShortName == templateParameterName)
                {
                    return argument;
                }
            }

            return null;
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

                    // Usually there are two quotes in the string expression
                    // But if it's a verbatim string, we should count @ symbol as well
                    var isVerbatimString = additiveArgument.Expression.IsVerbatimString();
                    var nonTemplateTokenCount = isVerbatimString ? 3 : 2;

                    // The token index is zero-based so we need to subtract 1
                    if (token.StartIndex < end - start - 1 - nonTemplateTokenCount + globalOffset)
                    {
                        var tokenStartIndex = start + token.StartIndex - globalOffset + 1;
                        if (isVerbatimString)
                        {
                            tokenStartIndex++;
                        }

                        var tokenEndIndex = tokenStartIndex + token.Length;

                        return (new TextRange(tokenStartIndex, end > tokenEndIndex ? tokenEndIndex : end), StringLiteralAltererUtil.TryCreateStringLiteralByExpression(additiveArgument.Expression));
                    }

                    globalOffset += end - start - nonTemplateTokenCount;
                }
            }

            var startOffset = argument.GetDocumentRange().TextRange.StartOffset + token.StartIndex + 1;
            if (argument.Expression.IsVerbatimString())
            {
                startOffset++;
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            return (new TextRange(startOffset, startOffset + token.Length), StringLiteralAltererUtil.TryCreateStringLiteralByExpression(argument.Expression));
        }

        public static string TryGetTemplateText(this ICSharpArgument argument)
        {
            if (argument.Value is IAdditiveExpression additiveExpression && additiveExpression.ConstantValue.IsString())
            {
                var linkedList = new LinkedList<ExpressionArgumentInfo>();
                FlattenAdditiveExpression(additiveExpression, linkedList);

                return string.Join(string.Empty, linkedList.Select(l => l.Expression.GetExpressionText()));
            }

            return argument.Value.GetExpressionText();
        }

        public static string GetExpressionText(this ICSharpExpression expression)
        {
            var stringLiteral = StringLiteralAltererUtil.TryCreateStringLiteralByExpression(expression);
            if (stringLiteral == null)
            {
                return null;
            }

            var expressionText = stringLiteral.Expression.GetText();
            if (expressionText.StartsWith("@"))
            {
                expressionText = expressionText.Substring(1);
            }

            return StringUtil.Unquote(expressionText);
        }

        [CanBeNull]
        public static IStringLiteralAlterer TryCreateLastTemplateFragmentExpression(this ICSharpArgument argument)
        {
            if (argument.Value is IAdditiveExpression additiveExpression && additiveExpression.ConstantValue.IsString())
            {
                var argumentInfo = additiveExpression.Arguments.Last();
                if (argumentInfo is ExpressionArgumentInfo expressionArgumentInfo)
                {
                    return StringLiteralAltererUtil.TryCreateStringLiteralByExpression(expressionArgumentInfo.Expression);
                }

                return null;
            }

            return argument.Value == null ? null : StringLiteralAltererUtil.TryCreateStringLiteralByExpression(argument.Value);
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

        public static bool IsSerilogContextPushPropertyMethod(this IInvocationExpression invocationExpression)
        {
            var typeMember = invocationExpression.Reference.Resolve().DeclaredElement as ITypeMember;
            var containingType = typeMember?.GetContainingType();
            if (containingType == null)
            {
                return false;
            }

            return LogContextFqn.Equals(containingType.GetClrName()) && typeMember.ShortName == "PushProperty";
        }

        public static bool IsVerbatimString([CanBeNull]this IExpression expression)
        {
            return expression?.FirstChild?.NodeType == CSharpTokenType.STRING_LITERAL_VERBATIM;
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
    }
}
