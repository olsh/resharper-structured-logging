using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using JetBrains.ReSharper.TestFramework;

using NUnit.Framework;

using ReSharper.Structured.Logging.QuickFixes;
using ReSharper.Structured.Logging.Tests.Constants;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    [TestFixture]
    [TestNetFramework46]
    [TestPackages(NugetPackages.SerilogNugetPackage)]
    public class RemoveDestructuringFromMessageTemplatePropertyFixAvailabilityTests
        : CSharpQuickFixAvailabilityTestBase<RemoveDestructuringFromMessageTemplatePropertyFix>
    {
        protected override string RelativeTestDataPath => @"QuickFixes\RemoveDestructuringFix";

        // The fix names the operator it removes
        [Test]
        public void TestSerilogDestructureOffered() => DoNamedTest();

        [Test]
        public void TestSerilogStringifyOffered() => DoNamedTest();
    }
}
