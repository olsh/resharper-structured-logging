using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    public class DuplicatePropertiesTemplateAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "DuplicatePropertiesTemplate";

        [Test] public void TestSerilogDuplicateNamedProperty() => DoNamedTest2();
    }
}
