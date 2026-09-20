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

        [Test] public void TestSerilogForContextWithoutDestructure() => DoNamedTest2();

        [Test] public void TestSerilogStaticLogForContextWithoutDestructure() => DoNamedTest2();

        // The concrete logger declares ForContext as its own member rather than inheriting the interface one
        [Test] public void TestSerilogCoreLoggerForContextWithoutDestructure() => DoNamedTest2();

        [Test] public void TestSerilogForContextExplicitDestructure() => DoNamedTest2();

        [Test] public void TestSerilogForContextNumericWithoutDestructure() => DoNamedTest2();

        // Two enrichers bind to the params overload, which has no flag to set; the type overloads take a single argument
        [Test] public void TestSerilogStaticLogForContextEnrichers() => DoNamedTest2();

        [Test] public void TestSerilogEnrichWithPropertyWithoutDestructure() => DoNamedTest2();

        [Test] public void TestSerilogCustomExceptionWithoutDestructure() => DoNamedTest2();

        [Test] public void TestSerilogParentWithOverriddenToString() => DoNamedTest2();

        [Test] public void TestSerilogNamedArgumentsWithoutDestructure() => DoNamedTest2();

        [Test] public void TestSerilogArrayArgumentsWithoutDestructure() => DoNamedTest2();
    }
}
