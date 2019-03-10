using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    public class AnonymousTypeDestructureAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        [Test]
        public void TestSerilogAnonymousTypeWithoutDestructure()
        {
            DoNamedTest2();
        }
    }
}
