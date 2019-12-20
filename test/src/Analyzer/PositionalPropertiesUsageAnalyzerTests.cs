using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    public class PositionalPropertiesUsageAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "PositionalPropertiesUsage";

        [Test] public void TestSerilogPositionProperty() => DoNamedTest2();
    }
}
