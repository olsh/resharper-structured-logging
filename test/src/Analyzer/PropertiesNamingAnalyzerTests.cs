using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    public class PropertiesNamingAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "PropertiesNamingAnalyzer";

        [Test] public void TestSerilogInvalidNamedProperty() => DoNamedTest2();

        [Test] public void TestSerilogValidNamedProperty() => DoNamedTest2();

        [Test] public void TestSerilogValidDestructuredNamedProperty() => DoNamedTest2();

        [Test] public void TestSerilogContextInvalidNamedProperty() => DoNamedTest2();

        [Test] public void TestSerilogContextInterpolatedStringProperty() => DoNamedTest2();

        [Test] public void TestSerilogInvalidNamedPropertyWithDot() => DoNamedTest2();

        [Test] public void TestSerilogInvalidNamedPropertyWithSpace() => DoNamedTest2();
    }
}
