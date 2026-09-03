using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using JetBrains.ReSharper.TestFramework;

using NUnit.Framework;

using ReSharper.Structured.Logging.QuickFixes;
using ReSharper.Structured.Logging.Tests.Constants;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    // A ZLogger 2.x template is an interpolated string, and every template fix rewrites its target as a
    // plain quoted literal, which would drop the `$` and the interpolations. The warnings are still
    // reported, the fixes just have to stay unavailable.
    // No Inherits on the package sets, so they replace the one on QuickFixTestBase: ZLogger 2.x needs
    // Microsoft.Extensions.Logging 8.0.0 and cannot be combined with the 6.0.0 pinned there
    [TestFixture]
    [TestNet80]
    [TestPackages(NugetPackages.ZLoggerV2LoggingPackage)]

    // ReSharper disable once TestFileNameWarning
    public class RenameLogPropertyFixAvailabilityTests : CSharpQuickFixAvailabilityTestBase<RenameLogPropertyFix>
    {
        protected override string RelativeTestDataPath => @"QuickFixes\RenameLogPropertyFix";

        [Test]
        public void TestZLoggerInterpolatedPropertyNotOffered() => DoNamedTest();
    }

    [TestFixture]
    [TestNet80]
    [TestPackages(NugetPackages.ZLoggerV2LoggingPackage)]

    // ReSharper disable once TestFileNameWarning
    public class RemoveTrailingPeriodFixAvailabilityTests
        : CSharpQuickFixAvailabilityTestBase<RemoveTrailingPeriodFix>
    {
        protected override string RelativeTestDataPath => @"QuickFixes\RemoveTrailingPeriodFix";

        [Test]
        public void TestZLoggerInterpolatedSentenceNotOffered() => DoNamedTest();
    }
}
