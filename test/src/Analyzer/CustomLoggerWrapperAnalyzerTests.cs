using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    public class CustomLoggerWrapperAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "CustomLoggerWrapper";

        [Test] public void TestMessageTemplateFormatMethodWrapper() => DoNamedTest2();

        [Test] public void TestStructuredMessageTemplateWrapper() => DoNamedTest2();
    }
}
