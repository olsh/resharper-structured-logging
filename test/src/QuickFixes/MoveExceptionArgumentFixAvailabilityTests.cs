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
    public class
        MoveExceptionArgumentFixAvailabilityTests : CSharpQuickFixAvailabilityTestBase<MoveExceptionArgumentFix>
    {
        protected override string RelativeTestDataPath => @"QuickFixes\MoveExceptionArgumentFix";

        [Test]
        public void TestSerilogExceptionAvailable() => DoNamedTest();

        // Moving the exception would pass two of them, the dedicated argument is already taken
        [Test]
        public void TestSerilogExceptionArgumentOccupiedNotAvailable() => DoNamedTest();

        [Test]
        public void TestSerilogExceptionArgumentOccupiedByNewExceptionNotAvailable() => DoNamedTest();

        [Test]
        public void TestSerilogExceptionMessageAvailable() => DoNamedTest();

        // Inserting the exception shifts the arguments along and drops a hole value, which resolves the
        // call to an overload that names its hole parameters differently, so a name the call keeps would
        // stop binding. Both warnings share the fix and therefore both withhold it here
        [Test]
        public void TestSerilogNamedArgumentMessageNotAvailable() => DoNamedTest();

        [Test]
        public void TestSerilogNamedArgumentExceptionNotAvailable() => DoNamedTest();
    }
}
