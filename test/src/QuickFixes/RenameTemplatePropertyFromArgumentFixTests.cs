using NUnit.Framework;

using ReSharper.Structured.Logging.QuickFixes;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    public class RenameTemplatePropertyFromArgumentFixTests : QuickFixTestBase<RenameTemplatePropertyFromArgumentFix>
    {
        protected override string SubPath => "RenameTemplatePropertyFromArgumentFix";

        [Test] public void TestSerilogPositionalProperty() => DoNamedTest();

        // The alignment and format of the hole survive the rename
        [Test] public void TestSerilogPositionalPropertyWithFormat() => DoNamedTest();
    }
}
