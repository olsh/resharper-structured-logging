using NUnit.Framework;

using ReSharper.Structured.Logging.QuickFixes;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    public class RemoveTrailingPeriodFixTests : QuickFixTestBase<RemoveTrailingPeriodFix>
    {
        protected override string SubPath => "RemoveTrailingPeriodFix";

        [Test] public void TestSerilogTrailingPeriod() => DoNamedTest2();
    }
}
