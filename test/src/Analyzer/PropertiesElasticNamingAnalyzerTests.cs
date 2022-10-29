using JetBrains.Application.Settings;

using NUnit.Framework;

using ReSharper.Structured.Logging.Settings;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    // ReSharper disable once TestFileNameWarning
    public class PropertiesElasticNamingAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "PropertiesNamingAnalyzer";

        [Test] public void TestSerilogInvalidElasticNamedProperty() => DoNamedTest2();

        protected override void MutateSettings(IContextBoundSettingsStore settingsStore)
        {
            settingsStore.SetValue<StructuredLoggingSettings, PropertyNamingType>(settings => settings.PropertyNamingType, PropertyNamingType.ElasticNaming);
        }
    }
}
