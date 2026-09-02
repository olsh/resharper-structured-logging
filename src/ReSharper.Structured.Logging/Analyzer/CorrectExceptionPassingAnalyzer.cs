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
            if (exceptionHoleIndex < 0)
            {
                return;
            }

            if (!IsExceptionOverloadAvailable(element, templateArgument, exceptionType))
            {
                return;
            }

            var namedProperty = TryGetHoleProperty(templateArgument, exceptionHoleIndex);
            var tokenInformation = namedProperty == null
                ? null
                : templateArgument.Value.GetTokenInformation(namedProperty);

            consumer.AddHighlighting(
                new ExceptionPassedAsTemplateArgumentWarning(
                    holeArguments[exceptionHoleIndex],
                    element,
                    tokenInformation,
                    namedProperty));
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
    }
}
