using NUnit.Framework;

using ReSharper.Structured.Logging.QuickFixes;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    public class RenameLogPropertyFixTests : QuickFixTestBase<RenameLogPropertyFix>
    {
        protected override string SubPath => "RenameLogPropertyFix";

        [Test] public void TestSerilogProperty() => DoNamedTest();

        [Test] public void TestSerilogDestructuredProperty() => DoNamedTest();

        [Test] public void TestSerilogPropertyConcatenated() => DoNamedTest();
    }
}
