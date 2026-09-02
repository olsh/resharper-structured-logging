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
    public class RenameLogPropertyFixDotNet6Tests : CSharpQuickFixTestBase<RenameLogPropertyFix>
    {
        protected override string RelativeTestDataPath => @"QuickFixes\RenameLogPropertyFix";

        [Test]
        public void TestLoggerMessageAttribute() => DoNamedTest();

        [Test]
        public void TestLoggerMessageAttributeConstructorArgument() => DoNamedTest();

        [Test]
        public void TestLoggerMessageDefine() => DoNamedTest();

        [Test]
        public void TestMicrosoftNamedMessageArgument() => DoNamedTest();
    }
}
