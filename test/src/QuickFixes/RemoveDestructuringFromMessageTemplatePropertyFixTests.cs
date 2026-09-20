using NUnit.Framework;

using ReSharper.Structured.Logging.QuickFixes;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    public class RemoveDestructuringFromMessageTemplatePropertyFixTests : QuickFixTestBase<RemoveDestructuringFromMessageTemplatePropertyFix>
    {
        protected override string SubPath => "RemoveDestructuringFix";

        [Test] public void TestSerilogDestructureString() => DoNamedTest();

        [Test] public void TestSerilogStringifyString() => DoNamedTest();

        [Test] public void TestSerilogEscapedString() => DoNamedTest();

        // The alignment and format of the hole stay in place, only the operator goes
        [Test] public void TestSerilogAlignmentAndFormat() => DoNamedTest();
    }
}
