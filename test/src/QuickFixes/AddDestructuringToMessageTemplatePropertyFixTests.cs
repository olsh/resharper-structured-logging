using NUnit.Framework;
using ReSharper.Structured.Logging.QuickFixes;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    public class AddDestructuringToMessageTemplatePropertyFixTests : QuickFixTestBase<AddDestructuringToMessageTemplatePropertyFix>
    {
        protected override string SubPath => "AddDestructuringFix";

        [Test] public void TestSerilogEscapedString() => DoNamedTest2();

        [Test] public void TestSerilogNewAnonymousObject() => DoNamedTest2();
    }
}
