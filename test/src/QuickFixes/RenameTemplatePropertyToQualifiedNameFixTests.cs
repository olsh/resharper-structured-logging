using NUnit.Framework;

using ReSharper.Structured.Logging.QuickFixes;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    public class RenameTemplatePropertyToQualifiedNameFixTests : QuickFixTestBase<RenameTemplatePropertyToQualifiedNameFix>
    {
        protected override string SubPath => "RenameTemplatePropertyToQualifiedNameFix";

        [Test] public void TestSerilogPositionalProperty() => DoNamedTest();
    }
}
