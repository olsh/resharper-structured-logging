using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using JetBrains.ReSharper.TestFramework;

using NUnit.Framework;

using ReSharper.Structured.Logging.ContextActions;
using ReSharper.Structured.Logging.Tests.Constants;

namespace ReSharper.Structured.Logging.Tests.ContextActions
{
    [TestFixture]
    [TestNet60]
    [TestPackages(NugetPackages.MicrosoftLoggingPackage, NugetPackages.SerilogNugetPackage)]
    public class ConvertToLoggerMessageContextActionAvailabilityTests
        : CSharpContextActionAvailabilityTestBase<ConvertToLoggerMessageContextAction>
    {
        protected override string ExtraPath => "ConvertToLoggerMessage";

        protected override string RelativeTestDataPath => @"ContextActions\ConvertToLoggerMessage";

        [Test]
        public void TestMicrosoftSimpleTemplateAvailable() => DoNamedTest();

        // TemplateIsNotCompileTimeConstantProblem has to be fixed before the template can move into an attribute
        [Test]
        public void TestMicrosoftInterpolatedTemplateNotAvailable() => DoNamedTest();

        // A single array passed to the params parameter hides the values that would become parameters
        [Test]
        public void TestMicrosoftArrayArgumentsNotAvailable() => DoNamedTest();

        // A null literal has no type to write in the signature
        [Test]
        public void TestMicrosoftNullArgumentNotAvailable() => DoNamedTest();

        // A scope is not a log event, so it has no [LoggerMessage] counterpart
        [Test]
        public void TestMicrosoftBeginScopeNotAvailable() => DoNamedTest();

        // The generator only reads the event id from the attribute, so a computed one cannot be carried over
        [Test]
        public void TestMicrosoftComputedEventIdNotAvailable() => DoNamedTest();

        // Two holes would end up sharing one parameter name
        [Test]
        public void TestMicrosoftDuplicatePropertiesNotAvailable() => DoNamedTest();

        // Serilog has no source generator to convert to
        [Test]
        public void TestSerilogNotAvailable() => DoNamedTest();
    }
}
