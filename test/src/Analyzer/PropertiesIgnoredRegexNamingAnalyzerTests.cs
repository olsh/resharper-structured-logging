using JetBrains.Application.Settings;

using NUnit.Framework;

using ReSharper.Structured.Logging.Settings;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    // ReSharper disable once TestFileNameWarning
    public class PropertiesIgnoredRegexNamingAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "PropertiesNamingAnalyzer";

        [Test] public void TestSerilogIgnoredInvalidNamedProperty() => DoNamedTest2();

        protected override void MutateSettings(IContextBoundSettingsStore settingsStore)
        {
            settingsStore.SetValue<StructuredLoggingSettings, string>(settings => settings.IgnoredPropertiesRegex,"MY_.*");
        }
    }
}
