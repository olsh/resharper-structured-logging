using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Impl.Resolve;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.CSharp.Util;
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
        private const string CreateLoggerMethodName = "CreateLogger";

        private const string ForContextMethodName = "ForContext";

        private const string PushPropertyMethodName = "PushProperty";

        private const string PushScopePropertyMethodName = "PushScopeProperty";

        private static readonly IClrTypeName LogContextFqn = new ClrTypeName("Serilog.Context.LogContext");

        private static readonly IClrTypeName LoggerFactoryExtensionsFqn =
            new ClrTypeName("Microsoft.Extensions.Logging.LoggerFactoryExtensions");

        private static readonly IClrTypeName LoggerMessageFqn =
            new ClrTypeName("Microsoft.Extensions.Logging.LoggerMessage");

        private static readonly IClrTypeName NLogLoggerFqn = new ClrTypeName("NLog.Logger");

        private static readonly IClrTypeName ScopeContextFqn = new ClrTypeName("NLog.ScopeContext");

        private static readonly IClrTypeName SerilogLogFqn = new ClrTypeName("Serilog.Log");

        private static readonly IClrTypeName SerilogLoggerFqn = new ClrTypeName("Serilog.ILogger");

        [CanBeNull]
        public static ICSharpArgument GetTemplateArgument(
            this IInvocationExpression invocationExpression,
            TemplateParameterNameAttributeProvider templateParameterNameAttributeProvider)
        {
            var templateParameterName =
                invocationExpression.GetTemplateParameterName(templateParameterNameAttributeProvider);

            return string.IsNullOrEmpty(templateParameterName)
                ? null
                : invocationExpression.FindTemplateArgument(templateParameterName);
        }

        /// <summary>
        /// Returns the arguments that fill the template holes, ordered by the parameter they are bound to,
        /// or <c>null</c> when the hole values cannot be tied to expressions because they were passed as a
        /// single array instead of being expanded.
        /// </summary>
        /// <remarks>
        /// A hole value is any argument bound to a parameter declared after the template parameter. That covers
        /// both the <c>params</c> overload and the generic ones Serilog resolves for short argument lists, and it
        /// excludes the dedicated exception slot, the event id and the extension receiver, which all come before
        /// the template. Binding through <see cref="ICSharpArgumentInfo.MatchingParameter"/> rather than through
        /// the argument position keeps the mapping correct for named, reordered and omitted optional arguments.
        /// </remarks>
        [CanBeNull]
        public static IReadOnlyList<ICSharpArgument> GetTemplateHoleArguments(
            this IInvocationExpression invocationExpression,
            [NotNull] ICSharpArgument templateArgument)
        {
            var templateParameter = templateArgument.MatchingParameter?.Element;
            if (templateParameter == null)
            {
                return null;
            }

            var templateParameterIndex = templateParameter.IndexOf();
            var holeArguments = new List<(int ParameterIndex, int ArgumentIndex, ICSharpArgument Argument)>();
            foreach (var argument in invocationExpression.ArgumentList.Arguments)
            {
                var parameterInstance = argument.MatchingParameter;
                var parameter = parameterInstance?.Element;
                if (parameter == null || !Equals(
                        parameter.ContainingParametersOwner,
                        templateParameter.ContainingParametersOwner))
                {
                    continue;
                }

                var parameterIndex = parameter.IndexOf();
                if (parameterIndex <= templateParameterIndex)
                {
                    continue;
                }

                // A single array passed to the params parameter hides the individual values,
                // so no hole can be tied to an expression
                if (parameter.IsParams && parameterInstance.Expanded != ArgumentsUtil.ExpandedKind.Expanded)
                {
                    return null;
                }

                holeArguments.Add((parameterIndex, argument.IndexOf(), argument));
            }

            // Several arguments share the parameter index when the params parameter is expanded,
            // their source order is the hole order
            return holeArguments
                .OrderBy(a => a.ParameterIndex)
                .ThenBy(a => a.ArgumentIndex)
                .Select(a => a.Argument)
                .ToArray();
        }

        /// <summary>
        /// The <see cref="ICSharpArgumentsOwner"/> overload of the hole binder. An attribute template has no
        /// arguments filling its holes, and the holes of LoggerMessage.Define are filled by generic type
        /// parameters, so both return <c>null</c>.
        /// </summary>
        [CanBeNull]
        public static IReadOnlyList<ICSharpArgument> GetTemplateHoleArguments(
            this ICSharpArgumentsOwner argumentsOwner,
            TemplateParameterNameAttributeProvider templateParameterNameAttributeProvider)
        {
            if (!(argumentsOwner is IInvocationExpression invocationExpression)
                || invocationExpression.IsLoggerMessageDefineMethod())
            {
                return null;
            }

            var templateArgument = invocationExpression.GetTemplateArgument(templateParameterNameAttributeProvider);

            return templateArgument == null
                ? null
                : invocationExpression.GetTemplateHoleArguments(templateArgument);
        }

        /// <summary>
        /// Returns the message template expression of a logging call or of a logging attribute
        /// such as [LoggerMessage(Message = "...")].
        /// </summary>
        [CanBeNull]
        public static ICSharpExpression GetTemplateExpression(
            this ICSharpArgumentsOwner argumentsOwner,
            TemplateParameterNameAttributeProvider templateParameterNameAttributeProvider)
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
                return attribute.FindTemplatePropertyAssignment(templateParameterName)
                    ?.Source;
            }

            return null;
        }

        public static MessageTemplateTokenInformation GetTokenInformation(
            this ICSharpExpression templateExpression,
            MessageTemplateToken token)
        {
            var (tokenTextRange, tokenArgument) = FindTokenTextRange(templateExpression, token);
            var tokenDocument = templateExpression.GetDocumentRange()
                .Document;
            var documentRange = new DocumentRange(tokenDocument, tokenTextRange);

            return new MessageTemplateTokenInformation(documentRange, tokenArgument);
        }

        private static (TextRange, IStringLiteralAlterer) FindTokenTextRange(
            this ICSharpExpression templateExpression,
            MessageTemplateToken token)
        {
            if (templateExpression is IAdditiveExpression additiveExpression &&
                additiveExpression.ConstantValue.IsString())
            {
                var concatenatedRange = FindTokenTextRangeInConcatenation(additiveExpression, token);
                if (concatenatedRange.HasValue)
                {
                    return concatenatedRange.Value;
                }
            }

            var startOffset = templateExpression.GetDocumentRange()
                .TextRange.StartOffset + token.StartIndex + 1;
            if (templateExpression.IsVerbatimString())
            {
                startOffset++;
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            return (new TextRange(startOffset, startOffset + token.Length),
                StringLiteralAltererUtil.TryCreateStringLiteralByExpression(templateExpression));
        }

        /// <summary>
        /// Walks the fragments of a concatenated template, tracking how many characters of the template each
        /// fragment contributes, and returns the range of the fragment the token falls into, or <c>null</c>
        /// when the token lies past the end of the concatenation.
        /// </summary>
        private static (TextRange, IStringLiteralAlterer)? FindTokenTextRangeInConcatenation(
            IAdditiveExpression additiveExpression,
            MessageTemplateToken token)
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

                    return (new TextRange(tokenStartIndex, end > tokenEndIndex ? tokenEndIndex : end),
                        StringLiteralAltererUtil.TryCreateStringLiteralByExpression(additiveArgument.Expression));
                }

                globalOffset += end - start - nonTemplateTokenCount;
            }

            return null;
        }

        public static string TryGetTemplateText(this ICSharpExpression templateExpression)
        {
            if (templateExpression is IAdditiveExpression additiveExpression &&
                additiveExpression.ConstantValue.IsString())
            {
                var linkedList = new LinkedList<ExpressionArgumentInfo>();
                FlattenAdditiveExpression(additiveExpression, linkedList);

                return string.Join(string.Empty, linkedList.Select(l => l.Expression.GetExpressionText()));
            }

            return templateExpression.GetExpressionText();
        }

        [CanBeNull]
        public static IStringLiteralAlterer TryCreateLastTemplateFragmentExpression(
            this ICSharpExpression templateExpression)
        {
            if (templateExpression is IAdditiveExpression additiveExpression &&
                additiveExpression.ConstantValue.IsString())
            {
                var arguments = additiveExpression.Arguments;
                var argumentInfo = arguments[arguments.Count - 1];
                if (argumentInfo is ExpressionArgumentInfo expressionArgumentInfo)
                {
                    return StringLiteralAltererUtil.TryCreateStringLiteralByExpression(
                        expressionArgumentInfo.Expression);
                }

                return null;
            }

            return templateExpression == null
                ? null
                : StringLiteralAltererUtil.TryCreateStringLiteralByExpression(templateExpression);
        }

        public static bool IsGenericMicrosoftExtensionsLogger([NotNull] this IDeclaredType declared)
        {
            return declared.GetClrName()
                .FullName == "Microsoft.Extensions.Logging.ILogger`1";
        }

        /// <summary>
        /// Matches the calls that build a logger categorised for a type: Serilog's
        /// <c>ILogger.ForContext&lt;T&gt;()</c> and the <c>Log.ForContext&lt;T&gt;()</c> static facade, and
        /// <c>ILoggerFactory.CreateLogger&lt;T&gt;()</c>. Requiring a single type argument leaves out the
        /// overloads that name the category some other way, such as <c>ForContext(string, object)</c>,
        /// <c>CreateLogger(string)</c> and the <c>Type</c> ones.
        /// </summary>
        public static bool IsContextualLoggerFactoryMethod([NotNull] this IInvocationExpression invocationExpression)
        {
            if (invocationExpression.TypeArguments.Count != 1)
            {
                return false;
            }

            var typeMember = invocationExpression.Reference?.Resolve()
                .DeclaredElement as ITypeMember;
            var containingType = typeMember?.GetContainingType();
            if (containingType == null)
            {
                return false;
            }

            var containingTypeName = containingType.GetClrName();
            if (SerilogLoggerFqn.Equals(containingTypeName) || SerilogLogFqn.Equals(containingTypeName))
            {
                return typeMember.ShortName == ForContextMethodName;
            }

            return LoggerFactoryExtensionsFqn.Equals(containingTypeName)
                   && typeMember.ShortName == CreateLoggerMethodName;
        }

        /// <summary>
        /// LoggerMessage.Define and DefineScope bind template holes to generic type parameters,
        /// so the arguments that follow the template are not the values of those holes.
        /// </summary>
        public static bool IsLoggerMessageDefineMethod(this IInvocationExpression invocationExpression)
        {
            var typeMember = invocationExpression.Reference?.Resolve()
                .DeclaredElement as ITypeMember;
            var containingType = typeMember?.GetContainingType();
            if (containingType == null)
            {
                return false;
            }

            return LoggerMessageFqn.Equals(containingType.GetClrName());
        }

        /// <summary>
        /// Serilog only, unlike <see cref="IsContextPushPropertyMethod"/>: the destructuring analysis this
        /// feeds is about the optional <c>destructureObjects</c> flag, which no other logger declares.
        /// </summary>
        public static bool IsSerilogContextPushPropertyMethod(this IInvocationExpression invocationExpression)
        {
            var typeMember = invocationExpression.Reference.Resolve()
                .DeclaredElement as ITypeMember;
            var containingType = typeMember?.GetContainingType();
            if (containingType == null)
            {
                return false;
            }

            return LogContextFqn.Equals(containingType.GetClrName()) && typeMember.ShortName == PushPropertyMethodName;
        }

        /// <summary>
        /// Matches the scope property calls that name the property in their first argument: Serilog's
        /// <c>LogContext.PushProperty</c>, NLog's <c>ScopeContext.PushProperty</c> and NLog's
        /// <c>Logger.PushScopeProperty</c>. The generic NLog overloads carry the same short name,
        /// so they are matched as well.
        /// </summary>
        public static bool IsContextPushPropertyMethod(this IInvocationExpression invocationExpression)
        {
            var typeMember = invocationExpression.Reference?.Resolve()
                .DeclaredElement as ITypeMember;
            var containingType = typeMember?.GetContainingType();
            if (containingType == null)
            {
                return false;
            }

            var containingTypeName = containingType.GetClrName();
            if (LogContextFqn.Equals(containingTypeName) || ScopeContextFqn.Equals(containingTypeName))
            {
                return typeMember.ShortName == PushPropertyMethodName;
            }

            return NLogLoggerFqn.Equals(containingTypeName) && typeMember.ShortName == PushScopePropertyMethodName;
        }

        [CanBeNull]
        public static IType GetFirstGenericArgumentType([NotNull] this IDeclaredType declared)
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
        public static ITypeUsage GetFirstTypeArgumentNode([CanBeNull] this ITypeUsage typeUsage)
        {
            return (typeUsage as IUserTypeUsage)?.ScalarTypeName?.TypeArgumentList?.TypeArgumentNodes
                .FirstOrDefault();
        }

        [CanBeNull]
        public static ITypeUsage GetFirstTypeArgumentNode([CanBeNull] this IInvocationExpression invocationExpression)
        {
            return (invocationExpression?.InvokedExpression as IReferenceExpression)?.TypeArgumentList
                ?.TypeArgumentNodes.FirstOrDefault();
        }

        private static bool IsVerbatimString([CanBeNull] this IExpression expression)
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

        /// <summary>
        /// Returns the name of the parameter holding the message template, or <c>null</c> when the invoked
        /// member is not a logging member. A <c>null</c> result is the plugin-wide "not a logging call" signal.
        /// </summary>
        [CanBeNull]
        public static string GetTemplateParameterName(
            this ICSharpArgumentsOwner argumentsOwner,
            TemplateParameterNameAttributeProvider templateParameterNameAttributeProvider)
        {
            // An attribute usage resolves the invoked constructor through a dedicated reference
            var declaredElement = argumentsOwner is IAttribute attribute
                ? attribute.ConstructorReference?.Resolve()
                    .DeclaredElement
                : argumentsOwner.Reference?.Resolve()
                    .DeclaredElement;

            return declaredElement is ITypeMember typeMember
                ? templateParameterNameAttributeProvider.GetInfo(typeMember)
                : null;
        }

        [CanBeNull]
        private static ICSharpArgument FindTemplateArgument(
            this ICSharpArgumentsOwner argumentsOwner,
            string templateParameterName)
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
        private static IPropertyAssignment FindTemplatePropertyAssignment(
            this IAttribute attribute,
            string templateParameterName)
        {
            // The attribute property mirrors the constructor parameter, e.g. `message` and `Message`
            return attribute.PropertyAssignments.FirstOrDefault(propertyAssignment => string.Equals(
                propertyAssignment.PropertyNameIdentifier?.Name,
                templateParameterName,
                StringComparison.OrdinalIgnoreCase));
        }

        private static void FlattenAdditiveExpression(
            IAdditiveExpression additiveExpression,
            LinkedList<ExpressionArgumentInfo> list)
        {
            foreach (var argumentInfo in additiveExpression.Arguments)
            {
                if (argumentInfo is ExpressionArgumentInfo expressionArgumentInfo &&
                    expressionArgumentInfo.Expression is IAdditiveExpression additive)
                {
                    FlattenAdditiveExpression(additive, list);

                    continue;
                }

                list.AddLast((ExpressionArgumentInfo)argumentInfo);
            }
        }
    }
}
