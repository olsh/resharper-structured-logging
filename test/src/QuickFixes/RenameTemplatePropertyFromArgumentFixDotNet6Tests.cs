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
    public class RenameTemplatePropertyFromArgumentFixDotNet6Tests : CSharpQuickFixTestBase<RenameTemplatePropertyFromArgumentFix>
    {
        protected override string RelativeTestDataPath => @"QuickFixes\RenameTemplatePropertyFromArgumentFix";

        // An attribute template has no argument to name the hole after, so the duplicate is numbered instead
        [Test]
        public void TestLoggerMessageAttributeDuplicateProperty() => DoNamedTest();
    }
}
