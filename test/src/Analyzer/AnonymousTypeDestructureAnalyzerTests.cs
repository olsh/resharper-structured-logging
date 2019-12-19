using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    public class AnonymousTypeDestructureAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "AnonymousTypeDestructure";

        [Test] public void TestSerilogWithoutDestructure() => DoNamedTest2();

        [Test] public void TestSerilogWithComplexPropertyWithoutDestructure() => DoNamedTest2();
    }
}
