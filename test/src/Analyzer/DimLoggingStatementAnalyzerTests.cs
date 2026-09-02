using JetBrains.Application.Settings;

using NUnit.Framework;

using ReSharper.Structured.Logging.Settings;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    public class DimLoggingStatementAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "DimLoggingStatement";

        [Test] public void TestSerilogLoggingStatement() => DoNamedTest2();

        [Test] public void TestLoggingCallInLambda() => DoNamedTest2();

        protected override void MutateSettings(IContextBoundSettingsStore settingsStore)
        {
            settingsStore.SetValue<StructuredLoggingSettings, bool>(settings => settings.DimLoggingStatements, true);
        }
    }
}
