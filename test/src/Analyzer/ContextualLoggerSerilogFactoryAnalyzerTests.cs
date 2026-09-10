using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    public class ContextualLoggerSerilogFactoryAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "ContextualLoggerSerilogFactory";

        [Test] public void TestSerilogCorrectContextType() => DoNamedTest2();

        [Test] public void TestSerilogWrongContextType() => DoNamedTest2();

        [Test] public void TestSerilogStaticLogCorrectContextType() => DoNamedTest2();

        [Test] public void TestSerilogStaticLogWrongContextType() => DoNamedTest2();

        [Test] public void TestSerilogStaticLogPassedToConstructorIsIgnored() => DoNamedTest2();

        [Test] public void TestSerilogStaticLogReturnedChainIsIgnored() => DoNamedTest2();

        [Test] public void TestSerilogStaticLogChainedCallIsReported() => DoNamedTest2();

        [Test] public void TestSerilogStaticLogInlineCallIsReported() => DoNamedTest2();

        [Test] public void TestSerilogStaticLogPropertyIsReported() => DoNamedTest2();
    }
}
