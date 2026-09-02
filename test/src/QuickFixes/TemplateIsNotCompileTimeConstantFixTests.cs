using NUnit.Framework;

using ReSharper.Structured.Logging.QuickFixes;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    public class TemplateIsNotCompileTimeConstantFixTests : QuickFixTestBase<TemplateIsNotCompileTimeConstantFix>
    {
        protected override string SubPath => "TemplateIsNotCompileTimeConstantFix";

        [Test] public void TestSerilogInterpolatedString() => DoNamedTest2();

        [Test] public void TestSerilogInterpolatedStringWithSingleQuotes() => DoNamedTest2();

        [Test] public void TestSerilogInterpolatedStringWithEscapeSequences() => DoNamedTest2();

        [Test] public void TestSerilogInterpolatedStringWithEscapedBraces() => DoNamedTest2();

        [Test] public void TestSerilogVerbatimInterpolatedString() => DoNamedTest2();
    }
}
