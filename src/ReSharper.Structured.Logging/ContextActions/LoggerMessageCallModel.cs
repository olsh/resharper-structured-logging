using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.Util;

using ReSharper.Structured.Logging.Caching;
using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Serilog.Events;
using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.ContextActions
{
    /// <summary>
    /// Everything the conversion reads off a Microsoft.Extensions.Logging call, gathered before anything is
    /// rewritten. A null result from <see cref="TryBuild"/> is the "cannot convert this call" signal, and it
    /// is what decides whether the context action offers itself at all.
    /// </summary>
    public sealed class LoggerMessageCallModel
    {
        private const string BeginScopeMethodName = "BeginScope";

        private const string EventIdParameterName = "eventId";

        private const string ExceptionParameterName = "exception";

        private const string LogLevelParameterName = "logLevel";

        private const string LogMethodName = "Log";

        private const string LoggerParameterName = "logger";

        private static readonly IClrTypeName LogLevelFqn = new ClrTypeName("Microsoft.Extensions.Logging.LogLevel");

        private static readonly IClrTypeName LoggerExtensionsFqn =
            new ClrTypeName("Microsoft.Extensions.Logging.LoggerExtensions");

        /// <summary>
        /// The parameter names the generated method keeps for the logger, the exception and the level. The
        /// generator binds those three by type and every other parameter by name, so a hole that would take
        /// one of these names is renamed instead.
        /// </summary>
        private static readonly string[] ReservedParameterNames = { "logger", "exception", "level" };

        private LoggerMessageCallModel(
            [NotNull] IInvocationExpression invocationExpression,
            [NotNull] ICSharpExpression loggerExpression,
            [NotNull] string messageLiteralText,
            [NotNull] string suggestedMethodName,
            [CanBeNull] string levelName,
            [CanBeNull] ICSharpExpression levelExpression,
            [CanBeNull] ICSharpExpression exceptionExpression,
            int? eventId,
            [NotNull] IReadOnlyList<LoggerMessageParameter> parameters)
        {
            InvocationExpression = invocationExpression;
            LoggerExpression = loggerExpression;
            MessageLiteralText = messageLiteralText;
            SuggestedMethodName = suggestedMethodName;
            LevelName = levelName;
            LevelExpression = levelExpression;
            ExceptionExpression = exceptionExpression;
            EventId = eventId;
            Parameters = parameters;
        }

        [NotNull]
        public IInvocationExpression InvocationExpression { get; }

        /// <summary>
        /// The logger the call was made on. For the usual extension-method form it is the qualifier of the
        /// invoked reference. An <c>ILogger&lt;T&gt;</c> is fine, since the generated method takes the
        /// non-generic <c>ILogger</c> it derives from.
        /// </summary>
        [NotNull]
        public ICSharpExpression LoggerExpression { get; }

        /// <summary>
        /// The C# expression to write after <c>Message =</c>, quotes and all. It is the text of the original
        /// template expression, carried over untouched so that its escapes, its verbatim prefix and any
        /// concatenation of literals still mean exactly what they meant at the call site.
        /// </summary>
        [NotNull]
        public string MessageLiteralText { get; }

        /// <summary>
        /// The name to give the generated method, derived from the words of the template. The action offers a
        /// rename hotspot on it, so it only has to be a reasonable opening bid.
        /// </summary>
        [NotNull]
        public string SuggestedMethodName { get; }

        /// <summary>
        /// The short name of the constant level, such as <c>Information</c>, or <c>null</c> when the level is
        /// not constant and <see cref="LevelExpression"/> carries it instead.
        /// </summary>
        [CanBeNull]
        public string LevelName { get; }

        /// <summary>
        /// A level the call computes rather than states. It becomes a <c>LogLevel</c> parameter of the
        /// generated method, which the generator also binds by type.
        /// </summary>
        [CanBeNull]
        public ICSharpExpression LevelExpression { get; }

        [CanBeNull]
        public ICSharpExpression ExceptionExpression { get; }

        /// <summary>
        /// The event id the call passes, when it passes a constant one. A call that passes none generates no
        /// <c>EventId</c>, which the generator has allowed since .NET 8.
        /// </summary>
        public int? EventId { get; }

        /// <summary>
        /// One parameter per template hole, in the order the holes bind to arguments.
        /// </summary>
        [NotNull]
        public IReadOnlyList<LoggerMessageParameter> Parameters { get; }

        [CanBeNull]
        public static LoggerMessageCallModel TryBuild(
            [CanBeNull] IInvocationExpression invocationExpression,
            [NotNull] TemplateParameterNameAttributeProvider templateParameterNameAttributeProvider,
            [NotNull] MessageTemplateParser messageTemplateParser)
        {
            if (invocationExpression == null || !invocationExpression.IsValid())
            {
                return null;
            }

            // Microsoft.Extensions.Logging only. The plugin-wide "this is a logging call" signal would also
            // answer for Serilog and NLog, which have no source generator, and for ZLogger, whose generator is
            // its own [ZLoggerMessage] rather than the one this action writes for.
            var method = invocationExpression.Reference?.Resolve()
                .DeclaredElement as IMethod;
            var containingType = method?.GetContainingType();
            if (containingType == null || !LoggerExtensionsFqn.Equals(containingType.GetClrName()))
            {
                return null;
            }

            // A scope is not a log event, so it has no [LoggerMessage] counterpart
            if (method.ShortName == BeginScopeMethodName)
            {
                return null;
            }

            var loggerExpression = FindLoggerExpression(invocationExpression);
            if (loggerExpression == null)
            {
                return null;
            }

            // A template that is neither a literal nor a concatenation of literals cannot move into an
            // attribute. TemplateIsNotCompileTimeConstantProblem is the inspection that asks for that first.
            var templateArgument = invocationExpression.GetTemplateArgument(templateParameterNameAttributeProvider);
            var templateText = templateArgument?.Value.TryGetTemplateText();
            if (templateText == null)
            {
                return null;
            }

            // Null when the hole values were passed as one array instead of being expanded, which hides them
            var holeArguments = invocationExpression.GetTemplateHoleArguments(templateArgument);
            if (holeArguments == null)
            {
                return null;
            }

            var level = TryReadLevel(method, invocationExpression);
            if (level == null)
            {
                return null;
            }

            var eventId = TryReadEventId(invocationExpression, out var eventIdIsUsable);
            if (!eventIdIsUsable)
            {
                return null;
            }

            var template = messageTemplateParser.Parse(templateText);
            var parameters = TryBuildParameters(template, holeArguments);
            if (parameters == null)
            {
                return null;
            }

            return new LoggerMessageCallModel(
                invocationExpression,
                loggerExpression,

                // The template is carried over exactly as it was written, which is the only way to be sure
                // that its escapes, its verbatim prefix and any concatenation still mean what they meant
                templateArgument.Value.GetText(),
                LoggerMessageMethodNameSuggestion.Suggest(template, "Log" + (level.Value.LevelName ?? "Message")),
                level.Value.LevelName,
                level.Value.LevelExpression,
                FindArgument(invocationExpression, ExceptionParameterName)
                    ?.Value,
                eventId,
                parameters);
        }

        public bool IsValid()
        {
            return InvocationExpression.IsValid()
                   && LoggerExpression.IsValid()
                   && (ExceptionExpression == null || ExceptionExpression.IsValid())
                   && (LevelExpression == null || LevelExpression.IsValid())
                   && Parameters.All(p => p.Value.IsValid() && p.Type.IsValid());
        }

        /// <summary>
        /// The parameter name of every hole of a named template, or <c>null</c> when two holes would end up
        /// sharing one, which is what DuplicateTemplatePropertyProblem asks to be fixed first.
        /// </summary>
        /// <remarks>
        /// Names are compared ignoring case, because that is how the generator matches a hole to a parameter:
        /// <c>{URL}</c> and <c>{Url}</c> would both bind to whichever parameter came first.
        /// </remarks>
        [CanBeNull]
        private static IReadOnlyList<string> BuildNamedHoleNames([CanBeNull] PropertyToken[] namedProperties)
        {
            if (namedProperties == null)
            {
                return Array.Empty<string>();
            }

            var names = new List<string>(namedProperties.Length);
            foreach (var namedProperty in namedProperties)
            {
                var name = ToParameterName(namedProperty.PropertyName);
                if (name == null || names.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    return null;
                }

                names.Add(name);
            }

            return names;
        }

        [CanBeNull]
        private static ICSharpArgument FindArgument(
            [NotNull] IInvocationExpression invocationExpression,
            [NotNull] string parameterName)
        {
            foreach (var argument in invocationExpression.ArgumentList.Arguments)
            {
                if (argument.MatchingParameter?.Element.ShortName == parameterName)
                {
                    return argument;
                }
            }

            return null;
        }

        /// <summary>
        /// The logger is the qualifier when the extension method is called as one, and an ordinary argument
        /// when it is called through <c>LoggerExtensions</c> by name.
        /// </summary>
        [CanBeNull]
        private static ICSharpExpression FindLoggerExpression([NotNull] IInvocationExpression invocationExpression)
        {
            var loggerArgument = FindArgument(invocationExpression, LoggerParameterName);

            return loggerArgument?.Value
                   ?? (invocationExpression.InvokedExpression as IReferenceExpression)?.QualifierExpression;
        }

        /// <summary>
        /// Reports whether the type can be named in the signature of the generated method. An unresolved type
        /// has no name to write, and neither has the anonymous type of a projection, whose compiler-generated
        /// name starts with a character no identifier may start with.
        /// </summary>
        private static bool IsWritableInSignature([NotNull] IType type)
        {
            if (type.IsVoid())
            {
                return false;
            }

            var typeElement = type.GetScalarType()
                ?.GetTypeElement();
            if (typeElement == null)
            {
                // An array, a pointer or a type parameter carries its own presentation rather than a type element
                return !(type is IDeclaredType);
            }

            return !typeElement.ShortName.StartsWith("<", StringComparison.Ordinal);
        }

        /// <summary>
        /// The short name of the <c>LogLevel</c> member the expression names, or <c>null</c> when the level is
        /// computed and has to travel as a parameter instead.
        /// </summary>
        [CanBeNull]
        private static string TryGetConstantLevelName([NotNull] ICSharpExpression expression)
        {
            if (!(expression is IReferenceExpression referenceExpression))
            {
                return null;
            }

            if (!(referenceExpression.Reference.Resolve()
                    .DeclaredElement is IField field))
            {
                return null;
            }

            return LogLevelFqn.Equals(
                field.GetContainingType()
                    ?.GetClrName())
                ? field.ShortName
                : null;
        }

        [CanBeNull]
        private static string TryGetLevelNameFromMethodName([NotNull] string methodName)
        {
            switch (methodName)
            {
                case "LogTrace": return "Trace";
                case "LogDebug": return "Debug";
                case "LogInformation": return "Information";
                case "LogWarning": return "Warning";
                case "LogError": return "Error";
                case "LogCritical": return "Critical";
                default: return null;
            }
        }

        /// <summary>
        /// Builds one parameter per hole, or <c>null</c> when the call cannot be converted.
        /// </summary>
        [CanBeNull]
        private static IReadOnlyList<LoggerMessageParameter> TryBuildParameters(
            [NotNull] MessageTemplate template,
            [NotNull] IReadOnlyList<ICSharpArgument> holeArguments)
        {
            // A template mixing {Named} and {0} holes is already broken, and which argument fills which hole
            // is guesswork, so it is left alone
            if (template.IsMixedTemplate)
            {
                return null;
            }

            // A positional template cannot survive the move. The generator matches holes to parameters by
            // name and nothing can be called 0, so the property keys would have to change, which quietly
            // rewrites whatever queries and dashboards read them. Renaming the holes is what
            // PositionalPropertyUsedProblem is for, and doing that first makes the call convertible.
            if (template.PositionalProperties != null)
            {
                return null;
            }

            var holeNames = BuildNamedHoleNames(template.NamedProperties);

            // A call passing more or fewer values than the template has holes is a defect of its own, and the
            // generated signature would bake it in
            if (holeNames == null || holeNames.Count != holeArguments.Count)
            {
                return null;
            }

            var parameters = new List<LoggerMessageParameter>(holeNames.Count);
            for (var index = 0; index < holeNames.Count; index++)
            {
                var holeArgument = holeArguments[index];
                var value = holeArgument.Value;
                if (value == null)
                {
                    return null;
                }

                // A null literal, a lambda and a method group have no type of their own, which ToIType
                // reports as null
                var type = holeArgument.GetExpressionType()
                    .ToIType();
                if (type == null || !IsWritableInSignature(type))
                {
                    return null;
                }

                parameters.Add(new LoggerMessageParameter(holeNames[index], type, value));
            }

            return parameters;
        }

        /// <summary>
        /// The event id the call passes. A non-constant one cannot be carried over, because the generator only
        /// reads it from the attribute, so <paramref name="isUsable"/> then reports that the call has to be
        /// refused rather than quietly losing it.
        /// </summary>
        private static int? TryReadEventId([NotNull] IInvocationExpression invocationExpression, out bool isUsable)
        {
            isUsable = true;
            var eventIdExpression = FindArgument(invocationExpression, EventIdParameterName)
                ?.Value;
            if (eventIdExpression == null)
            {
                return null;
            }

            // EventId converts implicitly from int, which is the only shape with a constant value to read
            if (eventIdExpression.ConstantValue.IsInteger(out var eventId))
            {
                return eventId;
            }

            isUsable = false;

            return null;
        }

        /// <summary>
        /// The level of the call, either as the constant name to write into the attribute or as the expression
        /// to turn into a parameter, or <c>null</c> when the method is not one whose level can be told, which
        /// is how an unknown <c>LoggerExtensions</c> member is refused.
        /// </summary>
        private static (string LevelName, ICSharpExpression LevelExpression)? TryReadLevel(
            [NotNull] IMethod method,
            [NotNull] IInvocationExpression invocationExpression)
        {
            var levelFromName = TryGetLevelNameFromMethodName(method.ShortName);
            if (levelFromName != null)
            {
                return (levelFromName, null);
            }

            if (method.ShortName != LogMethodName)
            {
                return null;
            }

            var levelExpression = FindArgument(invocationExpression, LogLevelParameterName)
                ?.Value;
            if (levelExpression == null)
            {
                return null;
            }

            var constantLevelName = TryGetConstantLevelName(levelExpression);

            return constantLevelName != null ? (constantLevelName, null) : (null, levelExpression);
        }

        /// <summary>
        /// The hole name as a C# parameter name, or <c>null</c> when nothing usable is left of it. The
        /// generator matches the two ignoring case, so lowering the first letter costs nothing.
        /// </summary>
        [CanBeNull]
        private static string ToParameterName([NotNull] string propertyName)
        {
            var name = StringUtil.MakeUpperCamelCaseName(propertyName)
                .Decapitalize();
            if (name.IsNullOrEmpty())
            {
                return null;
            }

            if (char.IsDigit(name[0]))
            {
                name = "arg" + name;
            }

            // The logger, the exception and the level are bound by type, so a hole may not take their names.
            // Case is ignored, since that is how the generator matches a hole to a parameter.
            if (ReservedParameterNames.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                name += "Value";
            }

            return CSharpLexer.IsKeyword(name) ? "@" + name : name;
        }
    }
}
