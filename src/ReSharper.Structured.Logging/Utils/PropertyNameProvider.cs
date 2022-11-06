using JetBrains.Annotations;
using JetBrains.Application.Settings;
using JetBrains.Util;

using ReSharper.Structured.Logging.Settings;

namespace ReSharper.Structured.Logging.Services;

public static class PropertyNameProvider
{
    public static string GetSuggestedName([NotNull]string propertyName, [CanBeNull]IContextBoundSettingsStore settingsStore)
    {
        var namingType = settingsStore
                             ?.GetValue(StructuredLoggingSettingsAccessor.PropertyNamingType)
                         ?? PropertyNamingType.PascalCase;

        switch (namingType)
        {
            case PropertyNamingType.PascalCase:
                return StringUtil.MakeUpperCamelCaseName(propertyName);
            case PropertyNamingType.CamelCase:
                return StringUtil.MakeUpperCamelCaseName(propertyName).Decapitalize();
            case PropertyNamingType.SnakeCase:
                return StringUtil.MakeUnderscoreCaseName(propertyName);
            case PropertyNamingType.ElasticNaming:
                return StringUtil.MakeUnderscoreCaseName(propertyName).Replace('_', '.');
            default:
                return propertyName;
        }
    }
}
