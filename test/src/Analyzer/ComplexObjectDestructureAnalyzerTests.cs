using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    public class ComplexObjectDestructureAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "ComplexTypeDestructure";

        [Test] public void TestSerilogWithoutDestructure() => DoNamedTest2();

        [Test] public void TestSerilogForceStringWithoutDestructure() => DoNamedTest2();

        [Test] public void TestSerilogNumericWithoutDestructure() => DoNamedTest2();

        [Test] public void TestSerilogEnumerableWithoutDestructure() => DoNamedTest2();

        [Test] public void TestSerilogNullableWithoutDestructure() => DoNamedTest2();

        [Test] public void TestSerilogDictionaryWithoutDestructure() => DoNamedTest2();
    }
}
