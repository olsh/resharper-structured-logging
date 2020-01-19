using JetBrains.Application.Settings;
using JetBrains.Application.Settings.WellKnownRootKeys;

namespace ReSharper.Structured.Logging.Settings
{
    [SettingsKey(typeof(EnvironmentSettings), "Settings for ConfigureAwait")]
    public class StructuredLoggingSettings
    {
        [SettingsEntry(PropertyNamingType.PascalCase, "Properties naming case")]
        public PropertyNamingType PropertyNamingType { get; set; }
    }
}
