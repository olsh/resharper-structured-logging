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
    }
}
