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
using ReSharper.Structured.Logging.Services;

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
            // The holes of a ZLogger 2.x template live inside the interpolated string, so the parameters
            // that follow it (context, memberName, filePath, lineNumber) hold no hole values
            if (templateArgument.Value.IsZLoggerTemplateHandler())
            {
                return null;
            }

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

        /// <summary>
        /// Returns the message template of a logging call or logging attribute, or <c>null</c> when the
        /// element is not a logging one or its template cannot be read. A template written as a string
        /// literal is parsed; a ZLogger 2.x template is recovered from its interpolated string.
        /// </summary>
        [CanBeNull]
        public static LogMessageTemplate TryGetLogMessageTemplate(
            this ICSharpArgumentsOwner argumentsOwner,
            TemplateParameterNameAttributeProvider templateParameterNameAttributeProvider,
            [NotNull] MessageTemplateParser messageTemplateParser)
        {
            var templateExpression = argumentsOwner.GetTemplateExpression(templateParameterNameAttributeProvider);
            if (templateExpression == null)
            {
                return null;
            }

            if (templateExpression is IInterpolatedStringExpression interpolatedString
                && interpolatedString.IsZLoggerTemplateHandler())
            {
                return InterpolatedMessageTemplateBuilder.TryBuild(interpolatedString);
            }

            var templateText = templateExpression.TryGetTemplateText();

            return templateText == null
                ? null
                : new LogMessageTemplate(templateExpression, messageTemplateParser.Parse(templateText));
        }

        /// <summary>
        /// Reports whether an expression is a ZLogger 2.x message template, that is an interpolated string
        /// bound to one of the <c>ZLogger*InterpolatedStringHandler</c> ref structs. An interpolated string
        /// handed to a plain <c>string</c> parameter, as in <c>Log.Information($"...")</c>, is not one: it is
        /// formatted before the logger ever sees it, which is what the compile time constant check is about.
        /// </summary>
        public static bool IsZLoggerTemplateHandler([CanBeNull] this ICSharpExpression expression)
        {
            if (!(expression is IInterpolatedStringExpression interpolatedString))
            {
                return false;
            }

            var handlerConstructor = interpolatedString.HandlerConstructorReference?.Resolve()
                .DeclaredElement as IConstructor;

            return ZLoggerTemplateHandler.IsHandlerType(handlerConstructor?.GetContainingType()
                ?.GetClrName());
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
            // A token of an interpolated template already carries the offset of the property name relative
            // to the start of the expression, so there is no opening quote to skip. There is no literal to
            // alter either, and a null alterer is what keeps the template quick fixes unavailable
            if (templateExpression is IInterpolatedStringExpression)
            {
                var nameStartOffset = templateExpression.GetDocumentRange()
                    .TextRange.StartOffset + token.StartIndex;

                return (new TextRange(nameStartOffset, nameStartOffset + token.Length), null);
            }

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
        /// Reports whether the logger the call produces ends up being the containing type's own logger.
        /// A logger that leaves the type - returned, handed to a constructor or a method, stored on some
        /// other object - was built on behalf of somebody else, which is what a factory method or a
        /// composition root does, and there the context type is meant to name that somebody.
        /// </summary>
        public static bool IsOwnLoggerOfContainingType(
            [NotNull] this IInvocationExpression invocationExpression,
            [NotNull] ITypeDeclaration typeDeclaration)
        {
            return StaysInContainingType(invocationExpression, typeDeclaration, null);
        }

        /// <summary>
        /// The three shapes that keep a logger where it was built: it initializes a member or a local, it
        /// is assigned to one, or it is spent on the spot as the qualifier of a further call. Anything
        /// else - an argument, a return, an object initializer, a lambda body - hands it to somebody
        /// else, so an unrecognized shape deliberately answers no: a missed warning beats a false one.
        /// </summary>
        private static bool StaysInContainingType(
            [NotNull] ICSharpExpression expression,
            [NotNull] ITypeDeclaration typeDeclaration,
            [CanBeNull] HashSet<IDeclaredElement> visitedVariables)
        {
            var value = GetConsumedExpression(expression);

            var initializer = ExpressionInitializerNavigator.GetByValue(value);
            if (initializer != null)
            {
                if (FieldDeclarationNavigator.GetByInitial(initializer) != null
                    || PropertyDeclarationNavigator.GetByInitial(initializer) != null)
                {
                    return true;
                }

                var variableDeclaration = LocalVariableDeclarationNavigator.GetByInitial(initializer);

                return variableDeclaration != null
                       && EveryReadStaysInContainingType(
                           variableDeclaration.DeclaredElement,
                           typeDeclaration,
                           visitedVariables);
            }

            var assignment = AssignmentExpressionNavigator.GetBySource(value);
            if (assignment != null)
            {
                return IsStoredInContainingType(assignment, typeDeclaration, visitedVariables);
            }

            return ReferenceExpressionNavigator.GetByQualifierExpression(value) != null;
        }

        /// <summary>
        /// Climbs to the expression that is actually consumed. Parentheses, a cast, the null forgiving
        /// operator, the branches of a conditional and the operands of ?? all pass the very same logger
        /// on, and so does a chained call handing back a logger of the same type: in a chain it is the
        /// last call that decides where the logger goes.
        /// </summary>
        [NotNull]
        private static ICSharpExpression GetConsumedExpression([NotNull] ICSharpExpression expression)
        {
            while (true)
            {
                var parent = GetValuePreservingParent(expression);
                if (parent == null)
                {
                    return expression;
                }

                expression = parent;
            }
        }

        [CanBeNull]
        private static ICSharpExpression GetValuePreservingParent([NotNull] ICSharpExpression expression)
        {
            var parenthesized = ParenthesizedExpressionNavigator.GetByExpression(expression);
            if (parenthesized != null)
            {
                return parenthesized;
            }

            var cast = CastExpressionNavigator.GetByOp(expression);
            if (cast != null)
            {
                return cast;
            }

            var suppressed = SuppressNullableWarningExpressionNavigator.GetByOperand(expression);
            if (suppressed != null)
            {
                return suppressed;
            }

            var conditional = ConditionalTernaryExpressionNavigator.GetByAnyBranch(expression);
            if (conditional != null)
            {
                return conditional;
            }

            var nullCoalescing = NullCoalescingExpressionNavigator.GetByAnyOperand(expression);
            if (nullCoalescing != null)
            {
                return nullCoalescing;
            }

            return GetChainedLoggerCall(expression);
        }

        /// <summary>
        /// The call written on top of this one when it hands back a logger of the same type. Comparing
        /// the types is what tells the Serilog ForContext("Job", id) that continues the chain from the
        /// Information(...) that spends it.
        /// </summary>
        [CanBeNull]
        private static IInvocationExpression GetChainedLoggerCall([NotNull] ICSharpExpression expression)
        {
            var qualifiedReference = ReferenceExpressionNavigator.GetByQualifierExpression(expression);
            if (qualifiedReference == null)
            {
                return null;
            }

            var chainedCall = InvocationExpressionNavigator.GetByInvokedExpression(qualifiedReference);

            return chainedCall != null && Equals(chainedCall.Type(), expression.Type()) ? chainedCall : null;
        }

        /// <summary>
        /// Whether an assignment parks the logger inside the containing type. A member of another type
        /// takes the logger away exactly like a constructor argument does; a local keeps it here only
        /// for as long as nothing reading it back carries it out.
        /// </summary>
        private static bool IsStoredInContainingType(
            [NotNull] IAssignmentExpression assignment,
            [NotNull] ITypeDeclaration typeDeclaration,
            [CanBeNull] HashSet<IDeclaredElement> visitedVariables)
        {
            // ??= fills a member the way = does, the arithmetic compound ones cannot apply to a logger
            if (assignment.AssignmentType != AssignmentType.EQ
                && assignment.AssignmentType != AssignmentType.DOUBLE_QUEST_EQ)
            {
                return false;
            }

            var target = (assignment.Dest as IReferenceExpression)?.Reference.Resolve()
                .DeclaredElement;
            if (target is ITypeMember member)
            {
                return Equals(member.GetContainingType(), typeDeclaration.DeclaredElement);
            }

            return target is ILocalVariable localVariable
                   && EveryReadStaysInContainingType(localVariable, typeDeclaration, visitedVariables);
        }

        /// <summary>
        /// Whether every read of the variable keeps the logger inside the containing type. The reads are
        /// found by walking the function the variable lives in, because an element problem analyzer has
        /// no search engine at hand; the writes are left out, otherwise the very assignment that led
        /// here would count as a read carrying the logger away.
        /// </summary>
        private static bool EveryReadStaysInContainingType(
            [CanBeNull] IDeclaredElement variable,
            [NotNull] ITypeDeclaration typeDeclaration,
            [CanBeNull] HashSet<IDeclaredElement> visitedVariables)
        {
            if (variable == null)
            {
                return false;
            }

            // A pair such as first = second; second = first; would otherwise never settle
            var visited = visitedVariables ?? new HashSet<IDeclaredElement>();
            if (!visited.Add(variable))
            {
                return true;
            }

            var declaration = variable.GetDeclarations()
                .FirstOrDefault();
            if (declaration == null)
            {
                return false;
            }

            // A local cannot be read outside the function it is declared in; one declared in a lambda
            // sitting in a member initializer has no function declaration to narrow the walk down to.
            var scope = (ITreeNode)declaration.GetContainingNode<ICSharpFunctionDeclaration>()
                        ?? typeDeclaration;
            foreach (var referenceExpression in scope.Descendants<IReferenceExpression>())
            {
                if (IsReadOf(referenceExpression, variable)
                    && !StaysInContainingType(referenceExpression, typeDeclaration, visited))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Whether the reference reads the variable rather than merely naming it as the target of a
        /// plain assignment. Leaving the stores out is what keeps the very assignment that led here
        /// from counting as a read that carries the logger away.
        /// </summary>
        private static bool IsReadOf(
            [NotNull] IReferenceExpression referenceExpression,
            [NotNull] IDeclaredElement variable)
        {
            // The cheap name check first, so that only the candidates are actually resolved
            if (referenceExpression.NameIdentifier?.Name != variable.ShortName)
            {
                return false;
            }

            if (!variable.Equals(
                    referenceExpression.Reference.Resolve()
                        .DeclaredElement))
            {
                return false;
            }

            var write = AssignmentExpressionNavigator.GetByDest(referenceExpression);

            return write == null || write.AssignmentType != AssignmentType.EQ;
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
