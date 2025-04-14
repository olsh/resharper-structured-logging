using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Application.Parts;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeAnnotations;

namespace ReSharper.Structured.Logging.Caching;

[CodeAnnotationProvider(Instantiation.DemandAnyThreadUnsafe)]
public class TemplateParameterNameAttributeProvider(
    AttributeInstancesProvider attributeInstancesProvider,
    CodeAnnotationsConfiguration codeAnnotationsConfiguration)
    : CodeAnnotationInfoProvider<ITypeMember, string>(attributeInstancesProvider, codeAnnotationsConfiguration, true)
{
    private const string MessageTemplateFormatMethodAttribute = "MessageTemplateFormatMethodAttribute";

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
}
