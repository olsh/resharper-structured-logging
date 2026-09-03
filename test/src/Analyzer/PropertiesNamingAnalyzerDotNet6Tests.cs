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

        // On ZLogger 1.x an interpolated string binds to the plain `string format` parameter, so it
        // is a formatted string rather than a template: it holds no properties to name, and it is
        // still reported as a template that is not compile time constant
        [Test] public void TestZLoggerInterpolatedTemplate() => DoNamedTest2();

        [Test] public void TestMicrosoftNamedMessageArgument() => DoNamedTest2();
    }
}
