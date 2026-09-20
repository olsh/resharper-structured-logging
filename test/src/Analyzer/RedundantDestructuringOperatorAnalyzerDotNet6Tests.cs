using JetBrains.ReSharper.TestFramework;

using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    // The Microsoft.Extensions.Logging extension methods only resolve on a modern target framework
    [TestNet60]
    public class RedundantDestructuringOperatorAnalyzerDotNet6Tests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "RedundantDestructuringOperatorDotNet6";

        [Test] public void TestMicrosoftDestructureString() => DoNamedTest2();
    }
}
