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

    // ReSharper disable once TestFileNameWarning
    public class RenameTemplatePropertyFromArgumentFixAvailabilityTests
        : CSharpQuickFixAvailabilityTestBase<RenameTemplatePropertyFromArgumentFix>
    {
        protected override string RelativeTestDataPath => @"QuickFixes\RenameTemplatePropertyFromArgumentFix";

        [Test]
        public void TestSerilogLeafNameOffered() => DoNamedTest();
    }

    [TestFixture]
    [TestNetFramework46]
    [TestPackages(NugetPackages.SerilogNugetPackage)]

    // ReSharper disable once TestFileNameWarning
    public class RenameTemplatePropertyToQualifiedNameFixAvailabilityTests
        : CSharpQuickFixAvailabilityTestBase<RenameTemplatePropertyToQualifiedNameFix>
    {
        protected override string RelativeTestDataPath => @"QuickFixes\RenameTemplatePropertyToQualifiedNameFix";

        // The qualified name differs from the leaf one, so it is offered as a second action
        [Test]
        public void TestSerilogQualifiedNameOffered() => DoNamedTest();

        // A plain identifier has a single name, so there is no second action to offer
        [Test]
        public void TestSerilogQualifiedNameNotOffered() => DoNamedTest();
    }
}
