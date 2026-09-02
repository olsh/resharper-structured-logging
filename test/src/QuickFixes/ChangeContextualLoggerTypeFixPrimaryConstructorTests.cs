using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using JetBrains.ReSharper.TestFramework;

using NUnit.Framework;

using ReSharper.Structured.Logging.QuickFixes;
using ReSharper.Structured.Logging.Tests.Constants;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    [TestFixture]
    [TestNet80]
    [TestPackages(NugetPackages.MicrosoftLoggingPackage)]
    public class ChangeContextualLoggerTypeFixPrimaryConstructorTests
        : CSharpQuickFixTestBase<ChangeContextualLoggerTypeFix>
    {
        protected override string RelativeTestDataPath => @"QuickFixes\ChangeContextualLoggerTypeFix";

        [Test] public void TestMicrosoftPrimaryConstructor() => DoNamedTest();

        [Test] public void TestRecordWrongContextType() => DoNamedTest();

        [Test] public void TestStructWrongContextType() => DoNamedTest();
    }
}
