using JetBrains.Application.Settings;

using NUnit.Framework;

using ReSharper.Structured.Logging.Settings;

namespace ReSharper.Structured.Logging.Tests.Completion
{
    // ReSharper disable once TestFileNameWarning
    public class TemplatePropertyCamelCaseCompletionTests : TemplatePropertyCompletionTestBase
    {
        protected override string SubPath => "TemplateProperty";

        [Test]
        public void TestSerilogCamelCaseNames() => DoNamedTest();

        protected override void MutateSettings(IContextBoundSettingsStore settingsStore)
        {
            settingsStore.SetValue<StructuredLoggingSettings, PropertyNamingType>(
                settings => settings.PropertyNamingType,
                PropertyNamingType.CamelCase);
        }
    }
}
