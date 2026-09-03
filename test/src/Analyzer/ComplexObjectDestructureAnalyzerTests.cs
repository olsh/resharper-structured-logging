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

        [Test] public void TestSerilogContextWithoutDestructure() => DoNamedTest2();

        [Test] public void TestSerilogContextNumericWithoutDestructure() => DoNamedTest2();

        [Test] public void TestSerilogContextExplicitDestructure() => DoNamedTest2();

        // The shape the destructuring quick fixes produce
        [Test] public void TestSerilogContextNamedExplicitDestructure() => DoNamedTest2();

        // NLog has no destructuring flag to add, so the warning stays away from its scope properties
        [Test] public void TestNlogScopeContextWithoutDestructure() => DoNamedTest2();

        [Test] public void TestSerilogCustomExceptionWithoutDestructure() => DoNamedTest2();

        [Test] public void TestSerilogParentWithOverriddenToString() => DoNamedTest2();

        [Test] public void TestSerilogNamedArgumentsWithoutDestructure() => DoNamedTest2();

        [Test] public void TestSerilogArrayArgumentsWithoutDestructure() => DoNamedTest2();
    }
}
