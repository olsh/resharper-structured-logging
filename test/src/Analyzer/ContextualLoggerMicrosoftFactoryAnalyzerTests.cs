using JetBrains.ReSharper.TestFramework;

using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    [TestNet60]
    public class ContextualLoggerMicrosoftFactoryAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "ContextualLoggerMicrosoftFactory";

        [Test] public void TestMicrosoftCorrectContextType() => DoNamedTest2();

        [Test] public void TestMicrosoftWrongContextType() => DoNamedTest2();

        [Test] public void TestMicrosoftGenericTypeParameterIsIgnored() => DoNamedTest2();
    }
}
