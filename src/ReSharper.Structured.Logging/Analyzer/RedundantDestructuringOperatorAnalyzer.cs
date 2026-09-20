using System;

using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
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
    /// <summary>
    /// Reports a destructuring or stringification operator on a hole whose value is a scalar. Serilog logs a
    /// string, a primitive, an enum, a <c>Guid</c>, a date, a time span or a <c>Uri</c> as a scalar whatever the
    /// operator says, and Microsoft.Extensions.Logging ignores the operator altogether, so the operator is
    /// noise at best and a sign the author expected an expansion that never happens at worst.
    /// </summary>
    [ElementProblemAnalyzer(typeof(IInvocationExpression), typeof(IAttribute))]
    public class RedundantDestructuringOperatorAnalyzer : ElementProblemAnalyzer<ICSharpArgumentsOwner>
    {
        private static readonly IClrTypeName UriFqn = new ClrTypeName("System.Uri");

        private readonly MessageTemplateParser _messageTemplateParser;

        private readonly Lazy<TemplateParameterNameAttributeProvider> _templateParameterNameAttributeProvider;

        public RedundantDestructuringOperatorAnalyzer(
            MessageTemplateParser messageTemplateParser,
            CodeAnnotationsCache codeAnnotationsCache)
        {
            _messageTemplateParser = messageTemplateParser;
            _templateParameterNameAttributeProvider =
                codeAnnotationsCache.GetLazyProvider<TemplateParameterNameAttributeProvider>();
        }

        protected override void Run(
            ICSharpArgumentsOwner element,
            ElementProblemAnalyzerData data,
            IHighlightingConsumer consumer)
        {
            var messageTemplate = element.TryGetLogMessageTemplate(
                _templateParameterNameAttributeProvider.Value,
                _messageTemplateParser);
            var namedProperties = messageTemplate?.Template.NamedProperties;
            if (namedProperties == null)
            {
                return;
            }

            var holeArguments = element.GetTemplateHoleArguments(_templateParameterNameAttributeProvider.Value);
            for (var index = 0; index < namedProperties.Length; index++)
            {
                var namedProperty = namedProperties[index];
                if (namedProperty.Destructuring == Destructuring.Default)
                {
                    continue;
                }

                var holeType = element.GetTemplateHoleType(namedProperty, index, holeArguments);
                if (holeType == null || !IsScalar(holeType))
                {
                    continue;
                }

                consumer.AddHighlighting(
                    new RedundantDestructuringOperatorWarning(
                        messageTemplate.Expression.GetTokenInformation(namedProperty),
                        namedProperty));
            }
        }

        /// <summary>
        /// The types Serilog always logs as a scalar. Anything else, including <c>object</c>, a type parameter
        /// and an interface, could hold a structured value at runtime and is left alone.
        /// </summary>
        private static bool IsScalar(IType type)
        {
            if (type.IsNullable())
            {
                var underlyingType = type.GetNullableUnderlyingType();

                return underlyingType != null && IsScalar(underlyingType);
            }

            // The predefined numeric types already include decimal and char
            return type.IsPredefinedNumeric()
                   || type.IsString()
                   || type.IsBool()
                   || type.IsGuid()
                   || type.IsDateTime()
                   || type.IsDateTimeOffset()
                   || type.IsTimeSpan()
                   || type.IsEnumType()
                   || type is IDeclaredType declaredType && Equals(declaredType.GetClrName(), UriFqn);
        }
    }
}
