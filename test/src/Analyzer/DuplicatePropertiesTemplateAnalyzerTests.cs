using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    public class DuplicatePropertiesTemplateAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        [Test]
        public void TestSerilogDuplicateNamedProperty()
        {
            DoNamedTest2();
        }
    }
}
