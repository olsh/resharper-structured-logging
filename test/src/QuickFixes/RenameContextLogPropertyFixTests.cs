using NUnit.Framework;

using ReSharper.Structured.Logging.QuickFixes;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    public class RenameContextLogPropertyFixTests : QuickFixTestBase<RenameContextLogPropertyFix>
    {
        protected override string SubPath => "RenameContextLogPropertyFix";

        [Test] public void TestSerilogContextProperty() => DoNamedTest();

        [Test] public void TestNlogScopeContextProperty() => DoNamedTest();

        [Test] public void TestNlogLoggerScopeProperty() => DoNamedTest();
    }
}
