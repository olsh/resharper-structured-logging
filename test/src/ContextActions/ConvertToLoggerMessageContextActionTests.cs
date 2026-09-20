using NUnit.Framework;

using ReSharper.Structured.Logging.ContextActions;

namespace ReSharper.Structured.Logging.Tests.ContextActions
{
    public class ConvertToLoggerMessageContextActionTests : ContextActionTestBase<ConvertToLoggerMessageContextAction>
    {
        protected override string SubPath => "ConvertToLoggerMessage";

        [Test]
        public void TestMicrosoftSimpleTemplate() => DoNamedTest();

        [Test]
        public void TestMicrosoftNoHoles() => DoNamedTest();

        // The exception travels as an Exception parameter, which the generator binds by type
        [Test]
        public void TestMicrosoftExceptionArgument() => DoNamedTest();

        [Test]
        public void TestMicrosoftConstantEventId() => DoNamedTest();

        // A level the call computes becomes a LogLevel parameter instead of an attribute value
        [Test]
        public void TestMicrosoftComputedLevel() => DoNamedTest();

        [Test]
        public void TestMicrosoftConstantLevelArgument() => DoNamedTest();

        // The generated method takes the non-generic ILogger the contextual logger derives from
        [Test]
        public void TestMicrosoftGenericLogger() => DoNamedTest();

        // An existing class carrying [LoggerMessage] members takes the new method rather than a second class
        [Test]
        public void TestMicrosoftExistingLogClass() => DoNamedTest();

        // A class named Log that is not a [LoggerMessage] holder must not be reused
        [Test]
        public void TestMicrosoftUnrelatedLogClass() => DoNamedTest();

        // {0} cannot name a parameter, so the holes are renamed along with the message
        [Test]
        public void TestMicrosoftPositionalTemplate() => DoNamedTest();

        // The destructuring operator, the alignment and the format stay in the message untouched
        [Test]
        public void TestMicrosoftHoleWithFormat() => DoNamedTest();

        [Test]
        public void TestMicrosoftStaticExtensionCall() => DoNamedTest();
    }
}
