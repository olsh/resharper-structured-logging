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

        if (className == "Microsoft.Extensions.Logging.LoggerMessage")
        {
            // Every Define and DefineScope overload declares the template as `formatString`
            return attributesOwner.ShortName is "Define" or "DefineScope" ? "formatString" : null;
        }

        if (className == "Microsoft.Extensions.Logging.LoggerMessageAttribute")
        {
            // The template is either the `message` constructor parameter or the `Message` property.
            // Every other member (EventId, EventName, Level, SkipEnabledCheck) holds no template.
            return attributesOwner is IConstructor || attributesOwner.ShortName == "Message" ? "message" : null;
        }

        return null;
    }

    protected override string GetDefaultInfo(ITypeMember attributesOwner)
    {
        return null;
    }
}
