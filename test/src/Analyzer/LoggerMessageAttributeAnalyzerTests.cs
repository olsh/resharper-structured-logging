using JetBrains.ReSharper.TestFramework;

using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    [TestNet60]
    public class LoggerMessageAttributeAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "LoggerMessageAttribute";

        [Test] public void TestNamedMessageInvalidProperty() => DoNamedTest2();

        [Test] public void TestPositionalMessageInvalidProperty() => DoNamedTest2();

        [Test] public void TestDuplicateProperties() => DoNamedTest2();

        [Test] public void TestPositionalProperty() => DoNamedTest2();

        [Test] public void TestMessageIsSentence() => DoNamedTest2();

        [Test] public void TestOtherAttributePropertiesIgnored() => DoNamedTest2();

        [Test] public void TestConcatenatedMessage() => DoNamedTest2();

        [Test] public void TestNonConstantMessage() => DoNamedTest2();

        [Test] public void TestMethodTargetAttribute() => DoNamedTest2();
    }
}
