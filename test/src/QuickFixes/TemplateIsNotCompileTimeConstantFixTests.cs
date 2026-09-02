using NUnit.Framework;

using ReSharper.Structured.Logging.QuickFixes;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    public class TemplateIsNotCompileTimeConstantFixTests : QuickFixTestBase<TemplateIsNotCompileTimeConstantFix>
    {
        protected override string SubPath => "TemplateIsNotCompileTimeConstantFix";

        [Test] public void TestSerilogInterpolatedString() => DoNamedTest();

        [Test] public void TestSerilogInterpolatedStringWithSingleQuotes() => DoNamedTest();

        [Test] public void TestSerilogInterpolatedStringWithEscapeSequences() => DoNamedTest();

        [Test] public void TestSerilogInterpolatedStringWithEscapedBraces() => DoNamedTest();

        [Test] public void TestSerilogVerbatimInterpolatedString() => DoNamedTest();
    }
}
