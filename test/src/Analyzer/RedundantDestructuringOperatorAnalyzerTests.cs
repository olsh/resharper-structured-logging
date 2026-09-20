using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    public class RedundantDestructuringOperatorAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "RedundantDestructuringOperator";

        [Test] public void TestSerilogDestructureString() => DoNamedTest2();

        [Test] public void TestSerilogStringifyString() => DoNamedTest2();

        [Test] public void TestSerilogScalarTypes() => DoNamedTest2();

        [Test] public void TestSerilogConcatenatedTemplate() => DoNamedTest2();

        [Test] public void TestNlogDestructureString() => DoNamedTest2();

        [Test] public void TestSerilogDestructureComplexObject() => DoNamedTest2();

        // These could hold a structured value at runtime, so the operator may well be doing something
        [Test] public void TestSerilogDestructureObjectParameter() => DoNamedTest2();

        [Test] public void TestSerilogDestructureGenericParameter() => DoNamedTest2();

        [Test] public void TestSerilogDestructureInterface() => DoNamedTest2();

        // Forcing the stringification of a type with its own ToString() is a deliberate choice
        [Test] public void TestSerilogStringifyOverriddenToString() => DoNamedTest2();

        [Test] public void TestSerilogDestructureCollection() => DoNamedTest2();

        // A single array passed to the params parameter hides the individual values
        [Test] public void TestSerilogArrayArguments() => DoNamedTest2();

        [Test] public void TestSerilogNullArgument() => DoNamedTest2();
    }
}
