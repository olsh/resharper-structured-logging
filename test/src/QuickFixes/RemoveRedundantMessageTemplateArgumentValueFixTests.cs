using NUnit.Framework;
using ReSharper.Structured.Logging.QuickFixes;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    public class RemoveRedundantMessageTemplateArgumentValueFixTests : QuickFixTestBase<RemoveRedundantMessageTemplateArgumentValueFix>
    {
        protected override string SubPath => "RedundantArgumentFix";

        [Test] public void TestSerilogRedundantArgument() => DoNamedTest2();
    }
}
