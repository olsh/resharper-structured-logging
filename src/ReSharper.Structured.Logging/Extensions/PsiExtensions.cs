using System;
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

        private static readonly IClrTypeName LoggerMessageFqn = new ClrTypeName("Microsoft.Extensions.Logging.LoggerMessage");

        [CanBeNull]
        public static ICSharpArgument GetTemplateArgument(this IInvocationExpression invocationExpression, TemplateParameterNameAttributeProvider templateParameterNameAttributeProvider)
        {
            var templateParameterName = invocationExpression.GetTemplateParameterName(templateParameterNameAttributeProvider);

            return string.IsNullOrEmpty(templateParameterName)
                       ? null
                       : invocationExpression.FindTemplateArgument(templateParameterName);
        }

        /// <summary>
        /// Returns the message template expression of a logging call or of a logging attribute
        /// such as [LoggerMessage(Message = "...")].
        /// </summary>
        [CanBeNull]
        public static ICSharpExpression GetTemplateExpression(this ICSharpArgumentsOwner argumentsOwner, TemplateParameterNameAttributeProvider templateParameterNameAttributeProvider)
        {
            var templateParameterName = argumentsOwner.GetTemplateParameterName(templateParameterNameAttributeProvider);
            if (string.IsNullOrEmpty(templateParameterName))
            {
                return null;
            }

            var templateArgument = argumentsOwner.FindTemplateArgument(templateParameterName);
            if (templateArgument != null)
            {
                return templateArgument.Value;
            }

            // An attribute can also carry the template in a named property, e.g. [LoggerMessage(Message = "...")]
            if (argumentsOwner is IAttribute attribute)
            {
                return attribute.FindTemplatePropertyAssignment(templateParameterName)?.Source;
            }

            return null;
        }

        public static MessageTemplateTokenInformation GetTokenInformation(this ICSharpExpression templateExpression, MessageTemplateToken token)
        {
            var (tokenTextRange, tokenArgument) = FindTokenTextRange(templateExpression, token);
            var tokenDocument = templateExpression.GetDocumentRange().Document;
            var documentRange = new DocumentRange(tokenDocument, tokenTextRange);

            return new MessageTemplateTokenInformation(documentRange, tokenArgument);
        }

        // ReSharper disable once CognitiveComplexity
        private static (TextRange, IStringLiteralAlterer) FindTokenTextRange(this ICSharpExpression templateExpression, MessageTemplateToken token)
        {
            if (templateExpression is IAdditiveExpression additiveExpression && additiveExpression.ConstantValue.IsString())
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

            var startOffset = templateExpression.GetDocumentRange().TextRange.StartOffset + token.StartIndex + 1;
            if (templateExpression.IsVerbatimString())
            {
                startOffset++;
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            return (new TextRange(startOffset, startOffset + token.Length), StringLiteralAltererUtil.TryCreateStringLiteralByExpression(templateExpression));
        }

        public static string TryGetTemplateText(this ICSharpExpression templateExpression)
        {
            if (templateExpression is IAdditiveExpression additiveExpression && additiveExpression.ConstantValue.IsString())
            {
                var linkedList = new LinkedList<ExpressionArgumentInfo>();
                FlattenAdditiveExpression(additiveExpression, linkedList);

                return string.Join(string.Empty, linkedList.Select(l => l.Expression.GetExpressionText()));
            }

            return templateExpression.GetExpressionText();
        }

        [CanBeNull]
        public static IStringLiteralAlterer TryCreateLastTemplateFragmentExpression(this ICSharpExpression templateExpression)
        {
            if (templateExpression is IAdditiveExpression additiveExpression && additiveExpression.ConstantValue.IsString())
            {
                var argumentInfo = additiveExpression.Arguments.Last();
                if (argumentInfo is ExpressionArgumentInfo expressionArgumentInfo)
                {
                    return StringLiteralAltererUtil.TryCreateStringLiteralByExpression(expressionArgumentInfo.Expression);
                }

                return null;
            }

            return templateExpression == null ? null : StringLiteralAltererUtil.TryCreateStringLiteralByExpression(templateExpression);
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

        /// <summary>
        /// LoggerMessage.Define and DefineScope bind template holes to generic type parameters,
        /// so the arguments that follow the template are not the values of those holes.
        /// </summary>
        public static bool IsLoggerMessageDefineMethod(this IInvocationExpression invocationExpression)
        {
            var typeMember = invocationExpression.Reference?.Resolve().DeclaredElement as ITypeMember;
            var containingType = typeMember?.GetContainingType();
            if (containingType == null)
            {
                return false;
            }

            return LoggerMessageFqn.Equals(containingType.GetClrName());
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

        [CanBeNull]
        public static ITypeUsage GetFirstTypeArgumentNode([CanBeNull]this ITypeUsage typeUsage)
        {
            return (typeUsage as IUserTypeUsage)?.ScalarTypeName?.TypeArgumentList?.TypeArgumentNodes
                .FirstOrDefault();
        }

        [CanBeNull]
        public static ITypeUsage GetFirstTypeArgumentNode([CanBeNull]this IInvocationExpression invocationExpression)
        {
            return (invocationExpression?.InvokedExpression as IReferenceExpression)?.TypeArgumentList
                ?.TypeArgumentNodes.FirstOrDefault();
        }

        private static bool IsVerbatimString([CanBeNull]this IExpression expression)
        {
            return expression?.FirstChild?.NodeType == CSharpTokenType.STRING_LITERAL_VERBATIM;
        }

        private static string GetExpressionText(this ICSharpExpression expression)
        {
            if (expression == null)
            {
                return null;
            }

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
        private static string GetTemplateParameterName(this ICSharpArgumentsOwner argumentsOwner, TemplateParameterNameAttributeProvider templateParameterNameAttributeProvider)
        {
            // An attribute usage resolves the invoked constructor through a dedicated reference
            var declaredElement = argumentsOwner is IAttribute attribute
                                      ? attribute.ConstructorReference?.Resolve().DeclaredElement
                                      : argumentsOwner.Reference?.Resolve().DeclaredElement;

            return declaredElement is ITypeMember typeMember
                       ? templateParameterNameAttributeProvider.GetInfo(typeMember)
                       : null;
        }

        [CanBeNull]
        private static ICSharpArgument FindTemplateArgument(this ICSharpArgumentsOwner argumentsOwner, string templateParameterName)
        {
            foreach (var argument in argumentsOwner.Arguments)
            {
                if (argument.MatchingParameter?.Element.ShortName == templateParameterName)
                {
                    return argument;
                }
            }

            return null;
        }

        [CanBeNull]
        private static IPropertyAssignment FindTemplatePropertyAssignment(this IAttribute attribute, string templateParameterName)
        {
            foreach (var propertyAssignment in attribute.PropertyAssignments)
            {
                // The attribute property mirrors the constructor parameter, e.g. `message` and `Message`
                if (string.Equals(propertyAssignment.PropertyNameIdentifier?.Name, templateParameterName, StringComparison.OrdinalIgnoreCase))
                {
                    return propertyAssignment;
                }
            }

            return null;
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
