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
    public class TemplateIsNotCompileTimeConstantFixAvailabilityTests : CSharpQuickFixAvailabilityTestBase<TemplateIsNotCompileTimeConstantFix>
    {
        protected override string RelativeTestDataPath => @"QuickFixes\TemplateIsNotCompileTimeConstantFix";

        [Test]
        public void TestMicrosoftLogAvailable() => DoNamedTest();

        // The fix would append the interpolation values as arguments, which LoggerMessage.Define cannot accept
        [Test]
        public void TestLoggerMessageDefineNotAvailable() => DoNamedTest();
    }
}
