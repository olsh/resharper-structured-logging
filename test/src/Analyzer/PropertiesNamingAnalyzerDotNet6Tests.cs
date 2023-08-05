using JetBrains.ReSharper.TestFramework;

using NUnit.Framework;

using ReSharper.Structured.Logging.Tests.Constants;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    [TestNet60]
    [TestPackages(
        NugetPackages.ZLoggerLoggingPackage,
        Inherits = true)]
    public class PropertiesNamingAnalyzerDotNet6Tests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "PropertiesNamingAnalyzerDotNet6";

        [Test] public void TestZLoggerInvalidNamedProperty() => DoNamedTest2();
    }
}
