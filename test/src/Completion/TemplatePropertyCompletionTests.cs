using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Completion
{
    public class TemplatePropertyCompletionTests : TemplatePropertyCompletionTestBase
    {
        protected override string SubPath => "TemplateProperty";

        [Test]
        public void TestSerilogFirstHole() => DoNamedTest();

        [Test]
        public void TestSerilogSecondHole() => DoNamedTest();

        // The hole that follows claims the last argument, so only the first one can name this hole
        [Test]
        public void TestSerilogHoleBeforeBoundHole() => DoNamedTest();

        // The same, with this hole left unterminated, which the parser reads as one run of text
        // together with the hole after it unless the unfinished brace is taken out of the way first
        [Test]
        public void TestSerilogHoleBeforeUnterminatedBoundHole() => DoNamedTest();

        [Test]
        public void TestSerilogDestructuringOperator() => DoNamedTest();

        [Test]
        public void TestSerilogStringificationOperator() => DoNamedTest();

        [Test]
        public void TestSerilogClosedHole() => DoNamedTest();

        // Re-completing a hole still offers the name it carries, which is the one it would be renamed to
        [Test]
        public void TestSerilogRecompletedHole() => DoNamedTest();

        [Test]
        public void TestSerilogTypedPrefix() => DoNamedTest();

        // order.Customer.Name offers both the qualified name and the leaf one
        [Test]
        public void TestSerilogQualifiedName() => DoNamedTest();

        [Test]
        public void TestSerilogNameAlreadyUsed() => DoNamedTest();

        // The caret sits right after an escaped {{, which opens no hole
        [Test]
        public void TestSerilogEscapedBraces() => DoNamedTest();

        [Test]
        public void TestSerilogHoleAfterEscapedBraces() => DoNamedTest();

        [Test]
        public void TestSerilogVerbatimTemplate() => DoNamedTest();

        [Test]
        public void TestMicrosoftLoggerHole() => DoNamedTest();

        [Test]
        public void TestNlogLoggerHole() => DoNamedTest();

        [Test]
        public void TestCustomLoggerWrapperHole() => DoNamedTest();

        // Every hole is bound already, so there is no argument left to name the one being typed
        [Test]
        public void TestSerilogNoUnboundArgument() => DoNamedTest();

        [Test]
        public void TestSerilogOutsideHole() => DoNamedTest();

        [Test]
        public void TestSerilogFormatSpecifier() => DoNamedTest();

        // A positional hole is a number, and renaming it is what the positional properties analyzer asks for
        [Test]
        public void TestSerilogPositionalHole() => DoNamedTest();

        // A single array passed to the params parameter hides the values the names would come from
        [Test]
        public void TestSerilogArrayArguments() => DoNamedTest();

        // The holes of an interpolated template are formatted before the logger sees them
        [Test]
        public void TestSerilogInterpolatedTemplate() => DoNamedTest();

        // A concatenated template is not supported yet: the hole index would need the per-fragment
        // arithmetic the token mapping does
        [Test]
        public void TestSerilogConcatenatedTemplate() => DoNamedTest();

        // ReSharper completes a [LoggerMessage] template from the parameters of the method it decorates
        [Test]
        public void TestLoggerMessageAttributeHole() => DoNamedTest();

        // The holes of LoggerMessage.Define are filled by generic type arguments, which name nothing
        [Test]
        public void TestLoggerMessageDefineHole() => DoNamedTest();
    }
}
