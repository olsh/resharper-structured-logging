using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeAnnotations;

namespace ReSharper.Structured.Logging.Caching
{
    [CodeAnnotationProvider]
    public class TemplateParameterNameAttributeProvider : CodeAnnotationInfoProvider<ITypeMember, string>
    {
        private const string MessageTemplateFormatMethodAttribute = "MessageTemplateFormatMethodAttribute";

        public TemplateParameterNameAttributeProvider(
            AttributeInstancesProvider attributeInstancesProvider,
            CodeAnnotationsConfiguration codeAnnotationsConfiguration)
            : base(attributeInstancesProvider, codeAnnotationsConfiguration, true)
        {
        }

        protected override string CalculateInfo(ITypeMember attributesOwner, IEnumerable<IAttributeInstance> attributeInstances)
        {
            var templateFormatAttribute = attributeInstances
                .FirstOrDefault(a => string.Equals(a.GetAttributeShortName(), MessageTemplateFormatMethodAttribute, StringComparison.Ordinal));

            if (templateFormatAttribute != null)
            {
                return templateFormatAttribute.PositionParameters()
                    .FirstOrDefault()
                    ?.ConstantValue.StringValue;
            }

            var className = attributesOwner.ContainingType?.GetClrName().FullName;
            if (className == "Microsoft.Extensions.Logging.LoggerExtensions")
            {
                return attributesOwner.ShortName == "BeginScope" ? "messageFormat" : "message";
            }

            if (className == "ZLogger.ZLoggerExtensions")
            {
                return "format";
            }

            return null;
        }

        protected override string GetDefaultInfo(ITypeMember attributesOwner)
        {
            return null;
        }

        public override string[] AttributeShortNames { get; } = { MessageTemplateFormatMethodAttribute };
    }
}
