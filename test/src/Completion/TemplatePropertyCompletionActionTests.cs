using JetBrains.ReSharper.FeaturesTestFramework.Completion;

using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Completion
{
    /// <summary>
    /// What accepting a name writes into the template. The list tests cover which names are offered and
    /// what range they replace, these two cover the brace the hole is still missing.
    /// </summary>
    // ReSharper disable once TestFileNameWarning
    public class TemplatePropertyCompletionActionTests : TemplatePropertyCompletionTestBase
    {
        protected override string SubPath => "TemplateProperty";

        protected override CodeCompletionTestType TestType => CodeCompletionTestType.Action;

        [Test]
        public void TestSerilogClosesHole() => DoNamedTest();

        [Test]
        public void TestSerilogKeepsClosingBrace() => DoNamedTest();

        // The brace belongs after the format, not between the name and it
        [Test]
        public void TestSerilogClosesHoleAfterFormat() => DoNamedTest();

        // A comma that begins no alignment is the punctuation of the message, so the hole closes in
        // front of it rather than swallowing it
        [Test]
        public void TestSerilogClosesHoleBeforePunctuation() => DoNamedTest();

        // A format may hold a space, and the brace that already closes the hole is past it
        [Test]
        public void TestSerilogKeepsClosingBraceAfterFormatWithSpace() => DoNamedTest();

        // The brace a space away closes nothing, so the hole gets one of its own and the escape after
        // it is left whole
        [Test]
        public void TestSerilogClosesHoleBeforeEscapedBrace() => DoNamedTest();
    }
}
