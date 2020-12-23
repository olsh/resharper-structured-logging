using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    public class LogMessageIsSentenceAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "LogMessageIsSentence";

        [Test] public void TestSerilogSentenceMessage() => DoNamedTest2();

        [Test] public void TestSerilogNotSentenceMessage() => DoNamedTest2();
    }
}
