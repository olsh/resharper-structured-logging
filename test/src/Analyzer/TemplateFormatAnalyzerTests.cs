using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    public class TemplateFormatAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        [Test]
        public void TestSerilogValidNamedProperty()
        {
            DoNamedTest2();
        }

        [Test]
        public void TestSerilogInValidNamedProperty()
        {
            DoNamedTest2();
        }

        [Test]
        public void TestSerilogInValidPositionProperty()
        {
            DoNamedTest2();
        }

        [Test]
        public void TestSerilogValidPositionProperty()
        {
            DoNamedTest2();
        }

        [Test]
        public void TestSerilogTemplateWithEscapedSymbols()
        {
            DoNamedTest2();
        }
    }
}
