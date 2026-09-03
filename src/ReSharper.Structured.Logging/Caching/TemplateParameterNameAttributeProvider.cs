using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;
using JetBrains.Application.Parts;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeAnnotations;

using ReSharper.Structured.Logging.Services;

namespace ReSharper.Structured.Logging.Caching;

[CodeAnnotationProvider(Instantiation.DemandAnyThreadUnsafe)]
public class TemplateParameterNameAttributeProvider(
    AttributeInstancesProvider attributeInstancesProvider,
    CodeAnnotationsConfiguration codeAnnotationsConfiguration)
    : CodeAnnotationInfoProvider<ITypeMember, string>(attributeInstancesProvider, codeAnnotationsConfiguration, true)
{
    private const string MessageTemplateFormatMethodAttribute = "MessageTemplateFormatMethodAttribute";

    private const string StructuredMessageTemplateAttribute = "StructuredMessageTemplateAttribute";

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
            // ZLogger 2.x replaced every `string format` overload with an interpolated string handler,
            // so the template moved from `format` to the handler parameter
            return FindInterpolatedStringHandlerParameterName(attributesOwner) ?? "format";
        }

        if (className == "Microsoft.Extensions.Logging.LoggerMessage")
        {
            // Every Define and DefineScope overload declares the template as `formatString`
            return attributesOwner.ShortName is "Define" or "DefineScope" ? "formatString" : null;
        }

        if (className is "Microsoft.Extensions.Logging.LoggerMessageAttribute" or "ZLogger.ZLoggerMessageAttribute")
        {
            // Both attributes declare the template either as the `message` constructor parameter or as the
            // `Message` property. Every other member (EventId, EventName, Level, SkipEnabledCheck) holds no
            // template, and the constructors that take no message simply have no `message` to resolve.
            return attributesOwner is IConstructor || attributesOwner.ShortName == "Message" ? "message" : null;
        }

        return FindStructuredMessageTemplateParameterName(attributesOwner);
    }

    protected override string GetDefaultInfo(ITypeMember attributesOwner)
    {
        return null;
    }

    /// <summary>
    /// The four supported libraries are recognized by class name, and a wrapper can carry the annotation on its
    /// template parameter rather than on the member. Neither case leaves an annotation attribute on the member
    /// itself, so the info has to be calculated even when the member carries no attributes.
    /// </summary>
    protected override bool ComputeWithoutAttributes()
    {
        return true;
    }

    /// <summary>
    /// Returns the name of the parameter typed as a ZLogger interpolated string handler, the shape every
    /// ZLogger 2.x <c>ZLog*</c> overload uses for its template, or <c>null</c> on the 1.x overloads, which
    /// take a plain <c>string format</c>. Matching the type rather than the parameter name keeps this
    /// correct if ZLogger ever renames the parameter.
    /// </summary>
    [CanBeNull]
    private static string FindInterpolatedStringHandlerParameterName(ITypeMember attributesOwner)
    {
        if (attributesOwner is not IParametersOwner parametersOwner)
        {
            return null;
        }

        foreach (var parameter in parametersOwner.Parameters)
        {
            if (parameter.Type is IDeclaredType declaredType
                && ZLoggerTemplateHandler.IsHandlerType(declaredType.GetClrName()))
            {
                return parameter.ShortName;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the name of the parameter marked with <c>[StructuredMessageTemplate]</c>, the JetBrains.Annotations
    /// attribute the built-in R#/Rider template highlighting also understands, or <c>null</c> when no parameter
    /// carries it. Only the attribute short name is matched, so a project can declare the attribute itself.
    /// </summary>
    private static string FindStructuredMessageTemplateParameterName(ITypeMember attributesOwner)
    {
        if (attributesOwner is not IParametersOwner parametersOwner)
        {
            return null;
        }

        foreach (var parameter in parametersOwner.Parameters)
        {
            var isTemplateParameter = parameter.GetAttributeInstances(AttributesSource.All)
                .Any(a => string.Equals(a.GetAttributeShortName(), StructuredMessageTemplateAttribute, StringComparison.Ordinal));

            if (isTemplateParameter)
            {
                return parameter.ShortName;
            }
        }

        return null;
    }
}
