using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    public class CorrectExceptionPassingAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        [Test]
        public void TestSerilogIncorrectExceptionPassing()
        {
            DoNamedTest2();
        }

        [Test]
        public void TestSerilogCorrectExceptionPassing()
        {
            DoNamedTest2();
        }

        [Test]
        public void TestSerilogMultipleExceptionPassing()
        {
            DoNamedTest2();
        }

        [Test]
        public void TestSerilogIncorrectExceptionPassingDynamicTemplate()
        {
            DoNamedTest2();
        }
    }
}
