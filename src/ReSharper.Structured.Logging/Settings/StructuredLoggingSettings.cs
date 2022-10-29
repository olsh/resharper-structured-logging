using JetBrains.Application.Settings;
using JetBrains.Application.Settings.WellKnownRootKeys;

namespace ReSharper.Structured.Logging.Settings
{
    [SettingsKey(typeof(EnvironmentSettings), "Settings for Structured Logging")]
    public class StructuredLoggingSettings
    {
        [SettingsEntry(PropertyNamingType.PascalCase, "Properties naming case")]
        public PropertyNamingType PropertyNamingType { get; set; }

        [SettingsEntry("", "Ignored properties RegEx")]
        public string IgnoredPropertiesRegex { get; set; }
    }
}
