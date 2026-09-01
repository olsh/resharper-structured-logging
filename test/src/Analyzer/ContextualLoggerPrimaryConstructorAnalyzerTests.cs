using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    [TestNet80]
    public class ContextualLoggerPrimaryConstructorAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "ContextualLoggerPrimaryConstructor";

        [Test]
        public void TestMicrosoftCorrectContextType() => DoNamedTest2();

        [Test]
        public void TestMicrosoftWrongContextType() => DoNamedTest2();

        [Test]
        public void TestMicrosoftWrongContextTypeMultipleParameters() => DoNamedTest2();

        [Test]
        public void TestRecordWrongContextType() => DoNamedTest2();

        [Test]
        public void TestStructWrongContextType() => DoNamedTest2();
    }
}
