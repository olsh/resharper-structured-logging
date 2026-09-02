using NUnit.Framework;

using ReSharper.Structured.Logging.QuickFixes;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    public class ChangeContextualLoggerTypeFixTests : QuickFixTestBase<ChangeContextualLoggerTypeFix>
    {
        protected override string SubPath => "ChangeContextualLoggerTypeFix";

        [Test] public void TestSerilogWrongContextType() => DoNamedTest2();
    }
}
