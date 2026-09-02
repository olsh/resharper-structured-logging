using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    // ReSharper disable once TestFileNameWarning
    public class DimLoggingStatementDisabledAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "DimLoggingStatement";

        [Test] public void TestDimmingDisabled() => DoNamedTest2();
    }
}
