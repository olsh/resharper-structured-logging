using JetBrains.Application.Settings;
using JetBrains.ReSharper.TestFramework;

using NUnit.Framework;

using ReSharper.Structured.Logging.Settings;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    [TestNet60]
    public class DimLoggingStatementDotNet6Tests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "DimLoggingStatementDotNet6";

        [Test] public void TestMicrosoftLoggingStatement() => DoNamedTest2();

        [Test] public void TestAwaitedLoggingStatement() => DoNamedTest2();

        protected override void MutateSettings(IContextBoundSettingsStore settingsStore)
        {
            settingsStore.SetValue<StructuredLoggingSettings, bool>(settings => settings.DimLoggingStatements, true);
        }
    }
}
