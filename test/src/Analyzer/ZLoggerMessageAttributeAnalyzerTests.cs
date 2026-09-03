using JetBrains.ReSharper.TestFramework;

using NUnit.Framework;

using ReSharper.Structured.Logging.Tests.Constants;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    // No Inherits, so this package set replaces the one on MessageTemplateAnalyzerTestBase: ZLogger 2.x
    // needs Microsoft.Extensions.Logging 8.0.0 and cannot be combined with the 6.0.0 pinned there
    [TestNet80]
    [TestPackages(NugetPackages.ZLoggerV2LoggingPackage)]
    public class ZLoggerMessageAttributeAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "ZLoggerMessageAttribute";

        [Test] public void TestNamedMessageInvalidProperty() => DoNamedTest2();

        [Test] public void TestPositionalMessageInvalidProperty() => DoNamedTest2();

        [Test] public void TestDuplicateProperties() => DoNamedTest2();

        [Test] public void TestMessageIsSentence() => DoNamedTest2();

        [Test] public void TestOtherAttributePropertiesIgnored() => DoNamedTest2();

        [Test] public void TestLevelOnlyConstructor() => DoNamedTest2();
    }
}
