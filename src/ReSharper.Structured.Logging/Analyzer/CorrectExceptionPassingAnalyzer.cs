using System;
using System.Collections.Generic;

using JetBrains.Annotations;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Util;

using ReSharper.Structured.Logging.Caching;
using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Highlighting;
using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.Analyzer
{
    [ElementProblemAnalyzer(typeof(IInvocationExpression))]
    public class CorrectExceptionPassingAnalyzer : ElementProblemAnalyzer<IInvocationExpression>
    {
        private readonly MessageTemplateParser _messageTemplateParser;

        private readonly Lazy<TemplateParameterNameAttributeProvider> _templateParameterNameAttributeProvider;

        public CorrectExceptionPassingAnalyzer(
            MessageTemplateParser messageTemplateParser,
            CodeAnnotationsCache codeAnnotationsCache)
        {
            _messageTemplateParser = messageTemplateParser;
            _templateParameterNameAttributeProvider =
                codeAnnotationsCache.GetLazyProvider<TemplateParameterNameAttributeProvider>();
        }

        protected override void Run(
            IInvocationExpression element,
            ElementProblemAnalyzerData data,
            IHighlightingConsumer consumer)
        {
            var templateArgument = element.GetTemplateArgument(_templateParameterNameAttributeProvider.Value);
            if (templateArgument == null)
            {
                return;
            }

            var exceptionType = element.PsiModule.GetPredefinedType()
                .TryGetType(PredefinedType.EXCEPTION_FQN, NullableAnnotation.Unknown);
            if (exceptionType == null)
            {
                return;
            }

            // Only the arguments bound to a parameter after the template are consumed as hole values.
            // An exception in the dedicated exception slot is bound before the template and is therefore
            // never a hole, no matter where it appears in the source
            var holeArguments = element.GetTemplateHoleArguments(templateArgument);
            if (holeArguments == null)
            {
                return;
            }

            var exceptionHoleIndex = FindExceptionHoleIndex(holeArguments, exceptionType);
            if (exceptionHoleIndex >= 0)
            {
                ReportExceptionPassedAsTemplateArgument(
                    element,
                    templateArgument,
                    holeArguments[exceptionHoleIndex],
                    exceptionHoleIndex,
                    exceptionType,
                    consumer);

                return;
            }

            // The exception object itself fills no hole here, but a piece of it logged as text loses it
            // just as thoroughly
            ReportExceptionLoggedAsText(element, templateArgument, holeArguments, exceptionType, consumer);
        }

        /// <summary>
        /// Reports whether an exception is already bound to a parameter before the template, that is whether the
        /// dedicated exception argument is taken.
        /// </summary>
        private static bool IsExceptionArgumentOccupied(
            [NotNull] IInvocationExpression invocationExpression,
            [NotNull] ICSharpArgument templateArgument,
            [NotNull] IDeclaredType exceptionType)
        {
            var templateParameter = templateArgument.MatchingParameter?.Element;
            if (templateParameter == null)
            {
                return false;
            }

            var templateParameterIndex = templateParameter.IndexOf();
            foreach (var argument in invocationExpression.ArgumentList.Arguments)
            {
                var parameter = argument.MatchingParameter?.Element;
                if (parameter == null || parameter.IndexOf() >= templateParameterIndex)
                {
                    continue;
                }

                if (argument.Value?.Type() is IDeclaredType declaredType && declaredType.IsSubtypeOf(exceptionType))
                {
                    return true;
                }
            }

            return false;
        }

        private static int FindExceptionHoleIndex(
            [NotNull] IReadOnlyList<ICSharpArgument> holeArguments,
            [NotNull] IDeclaredType exceptionType)
        {
            for (var index = 0; index < holeArguments.Count; index++)
            {
                if (holeArguments[index]
                        .Value?.Type() is IDeclaredType declaredType
                    && declaredType.IsSubtypeOf(exceptionType))
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Reports whether the exception could be moved at all, that is whether any candidate overload declares
        /// an exception parameter before the template parameter. Both indices are declaration positions, so this
        /// stays correct for named and reordered arguments.
        /// </summary>
        private static bool IsExceptionOverloadAvailable(
            [NotNull] IInvocationExpression invocationExpression,
            [NotNull] ICSharpArgument templateArgument,
            [NotNull] IDeclaredType exceptionType)
        {
            var templateParameterName = templateArgument.MatchingParameter?.Element.ShortName;
            if (templateParameterName == null)
            {
                return false;
            }

            foreach (var candidate in invocationExpression.InvocationExpressionReference.GetCandidates())
            {
                if (!(candidate.GetDeclaredElement() is IMethod declaredElement))
                {
                    continue;
                }

                var parameters = declaredElement.Parameters;
                for (var index = 0; index < parameters.Count; index++)
                {
                    if (parameters[index].ShortName == templateParameterName)
                    {
                        break;
                    }

                    if (parameters[index]
                        .Type.IsSubtypeOf(exceptionType))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the first hole filled with the text of an exception, together with the exception that text
        /// was read from. A <c>null</c> expression means there is no such hole.
        /// </summary>
        private static (int HoleIndex, ICSharpExpression ExceptionExpression) FindExceptionTextHole(
            [NotNull] IReadOnlyList<ICSharpArgument> holeArguments,
            [NotNull] IDeclaredType exceptionType)
        {
            for (var index = 0; index < holeArguments.Count; index++)
            {
                var exceptionExpression = TryGetLoggedExceptionExpression(holeArguments[index].Value, exceptionType);
                if (exceptionExpression != null)
                {
                    return (index, exceptionExpression);
                }
            }

            return (-1, null);
        }

        /// <summary>
        /// Returns the exception a template argument reads its text from, or <c>null</c> when the argument is
        /// none of the three members that lose it. <c>exception.Data["key"]</c>, <c>exception.HResult</c> and a
        /// property declared by a derived exception are legitimate scalars to log and are left alone, and so is
        /// an unqualified <c>Message</c> read inside an exception class, which has no receiver to move.
        /// </summary>
        [CanBeNull]
        private static ICSharpExpression TryGetLoggedExceptionExpression(
            [CanBeNull] ICSharpExpression argumentValue,
            [NotNull] IDeclaredType exceptionType)
        {
            // The cheap name check comes first, so that only the candidates have their qualifier resolved
            switch (argumentValue)
            {
                // Only the parameterless ToString() renders the whole exception
                case IInvocationExpression invocationExpression:
                    return invocationExpression.Arguments.Count == 0
                           && invocationExpression.InvokedExpression is IReferenceExpression invokedReference
                           && invokedReference.NameIdentifier?.Name == nameof(Exception.ToString)
                        ? GetExceptionQualifier(invokedReference, exceptionType)
                        : null;

                case IReferenceExpression referenceExpression:
                    var memberName = referenceExpression.NameIdentifier?.Name;

                    return memberName == nameof(Exception.Message) || memberName == nameof(Exception.StackTrace)
                        ? GetExceptionQualifier(referenceExpression, exceptionType)
                        : null;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns the qualifier of a member access when it is an exception, so that a chain such as
        /// <c>exception.GetBaseException().Message</c> hands back the call the text was read from.
        /// </summary>
        [CanBeNull]
        private static ICSharpExpression GetExceptionQualifier(
            [NotNull] IReferenceExpression referenceExpression,
            [NotNull] IDeclaredType exceptionType)
        {
            var qualifier = referenceExpression.QualifierExpression;

            return qualifier?.Type() is IDeclaredType declaredType && declaredType.IsSubtypeOf(exceptionType)
                ? qualifier
                : null;
        }

        /// <summary>
        /// Returns the template hole the exception fills, or <c>null</c> when the template is not a literal whose
        /// holes can be located. The quick fix then moves the argument without touching the message.
        /// </summary>
        [CanBeNull]
        private PropertyToken TryGetHoleProperty([NotNull] ICSharpArgument templateArgument, int holeIndex)
        {
            if (templateArgument.Value is IInterpolatedStringExpression)
            {
                return null;
            }

            var templateText = templateArgument.Value.TryGetTemplateText();
            if (templateText == null)
            {
                return null;
            }

            var messageTemplate = _messageTemplateParser.Parse(templateText);
            if (messageTemplate.NamedProperties == null || holeIndex >= messageTemplate.NamedProperties.Length)
            {
                return null;
            }

            return messageTemplate.NamedProperties[holeIndex];
        }

        private void ReportExceptionPassedAsTemplateArgument(
            [NotNull] IInvocationExpression element,
            [NotNull] ICSharpArgument templateArgument,
            [NotNull] ICSharpArgument exceptionArgument,
            int holeIndex,
            [NotNull] IDeclaredType exceptionType,
            [NotNull] IHighlightingConsumer consumer)
        {
            if (!IsExceptionOverloadAvailable(element, templateArgument, exceptionType))
            {
                return;
            }

            var namedProperty = TryGetHoleProperty(templateArgument, holeIndex);
            var tokenInformation = namedProperty == null
                ? null
                : templateArgument.Value.GetTokenInformation(namedProperty);

            consumer.AddHighlighting(
                new ExceptionPassedAsTemplateArgumentWarning(
                    exceptionArgument,
                    templateArgument,
                    element,
                    tokenInformation,
                    namedProperty,
                    IsExceptionArgumentOccupied(element, templateArgument, exceptionType)));
        }

        /// <summary>
        /// Reports a piece of an exception logged as text, such as <c>exception.Message</c>. The event then
        /// carries no exception at all, so a sink can neither group nor render it as one. Nothing is reported
        /// once the dedicated exception argument is taken: the exception is not lost then, and the repeated
        /// text is a deliberate choice often enough to leave alone.
        /// </summary>
        private void ReportExceptionLoggedAsText(
            [NotNull] IInvocationExpression element,
            [NotNull] ICSharpArgument templateArgument,
            [NotNull] IReadOnlyList<ICSharpArgument> holeArguments,
            [NotNull] IDeclaredType exceptionType,
            [NotNull] IHighlightingConsumer consumer)
        {
            var (holeIndex, exceptionExpression) = FindExceptionTextHole(holeArguments, exceptionType);
            if (exceptionExpression == null
                || !IsExceptionOverloadAvailable(element, templateArgument, exceptionType)
                || IsExceptionArgumentOccupied(element, templateArgument, exceptionType))
            {
                return;
            }

            var namedProperty = TryGetHoleProperty(templateArgument, holeIndex);
            var tokenInformation = namedProperty == null
                ? null
                : templateArgument.Value.GetTokenInformation(namedProperty);

            consumer.AddHighlighting(
                new ExceptionLoggedAsTextWarning(
                    holeArguments[holeIndex],
                    exceptionExpression,
                    templateArgument,
                    element,
                    tokenInformation,
                    namedProperty));
        }
    }
}
