using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    public class CompileTimeConstantTemplateAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "CompileTimeConstantTemplate";

        [Test] public void TestSerilogInterpolatedString() => DoNamedTest2();

        [Test] public void TestSerilogConcatenatedString() => DoNamedTest2();

        [Test] public void TestSerilogStringFormat() => DoNamedTest2();

        [Test] public void TestSerilogVariableTemplate() => DoNamedTest2();

        [Test] public void TestSerilogConstantTemplate() => DoNamedTest2();
    }
}
