using NUnit.Framework;

using ReSharper.Structured.Logging.QuickFixes;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    public class RenameLogPropertyFixTests : QuickFixTestBase<RenameLogPropertyFix>
    {
        protected override string SubPath => "RenameLogPropertyFix";

        [Test] public void TestSerilogProperty() => DoNamedTest2();

        [Test] public void TestSerilogDestructuredProperty() => DoNamedTest2();

        [Test] public void TestSerilogPropertyConcatenated() => DoNamedTest2();
    }
}
