using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    public class ContextualLoggerSerilogFactoryAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "ContextualLoggerSerilogFactory";

        [Test] public void TestSerilogCorrectContextType() => DoNamedTest2();

        [Test] public void TestSerilogWrongContextType() => DoNamedTest2();
    }
}
