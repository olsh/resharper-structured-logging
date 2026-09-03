using NUnit.Framework;

using ReSharper.Structured.Logging.QuickFixes;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    public class MoveExceptionArgumentFixTests : QuickFixTestBase<MoveExceptionArgumentFix>
    {
        protected override string SubPath => "MoveExceptionArgumentFix";

        [Test] public void TestSerilogTrailingExceptionProperty() => DoNamedTest();

        [Test] public void TestSerilogExceptionPropertyInTheMiddle() => DoNamedTest();

        [Test] public void TestSerilogDynamicTemplate() => DoNamedTest();
    }
}
