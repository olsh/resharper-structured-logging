using JetBrains.ReSharper.TestFramework;

using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    [TestNet60]
    public class LoggerMessageDefineAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "LoggerMessageDefine";

        [Test] public void TestDefineInvalidProperty() => DoNamedTest2();

        [Test] public void TestDefineScopeInvalidProperty() => DoNamedTest2();

        [Test] public void TestDefineDuplicateProperties() => DoNamedTest2();

        [Test] public void TestDefinePositionalProperty() => DoNamedTest2();

        [Test] public void TestDefineMessageIsSentence() => DoNamedTest2();

        [Test] public void TestDefineConcatenatedMessage() => DoNamedTest2();

        [Test] public void TestDefineWithOptions() => DoNamedTest2();

        [Test] public void TestDefineScopeValidProperty() => DoNamedTest2();
    }
}
