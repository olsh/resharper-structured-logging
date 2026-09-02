using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using ReSharper.Structured.Logging.QuickFixes;
using ReSharper.Structured.Logging.Tests.Constants;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    [TestFixture]
    [TestNet60]
    [TestPackages(NugetPackages.MicrosoftLoggingPackage)]
    public class RemoveTrailingPeriodFixDotNet6Tests : CSharpQuickFixTestBase<RemoveTrailingPeriodFix>
    {
        protected override string RelativeTestDataPath => @"QuickFixes\RemoveTrailingPeriodFix";

        [Test]
        public void TestMicrosoftNamedMessageArgument() => DoNamedTest();

        [Test]
        public void TestLoggerMessageAttribute() => DoNamedTest();
    }
}
