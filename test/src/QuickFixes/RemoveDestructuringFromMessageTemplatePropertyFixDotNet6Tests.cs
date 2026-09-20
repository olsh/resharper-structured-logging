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
    public class RemoveDestructuringFromMessageTemplatePropertyFixDotNet6Tests
        : CSharpQuickFixTestBase<RemoveDestructuringFromMessageTemplatePropertyFix>
    {
        protected override string RelativeTestDataPath => @"QuickFixes\RemoveDestructuringFix";

        [Test]
        public void TestLoggerMessageAttribute() => DoNamedTest();

        [Test]
        public void TestLoggerMessageDefine() => DoNamedTest();
    }
}
