using JetBrains.ReSharper.TestFramework;

using NUnit.Framework;

using ReSharper.Structured.Logging.Tests.Constants;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    // No Inherits, so this package set replaces the one on MessageTemplateAnalyzerTestBase: ZLogger 2.x
    // needs Microsoft.Extensions.Logging 8.0.0 and cannot be combined with the 6.0.0 pinned there
    [TestNet80]
    [TestPackages(NugetPackages.ZLoggerV2LoggingPackage)]
    public class ZLoggerV2AnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "ZLoggerV2";

        [Test] public void TestInvalidNamedProperty() => DoNamedTest2();

        [Test] public void TestExplicitNameInvalidProperty() => DoNamedTest2();

        // The name ends at the first colon after the '@', the rest is the real format
        [Test] public void TestExplicitNameWithFormat() => DoNamedTest2();

        // ':json' is a format, not a name, so the hole keeps the name of its expression
        [Test] public void TestJsonFormatKeepsName() => DoNamedTest2();

        [Test] public void TestAlignmentIgnored() => DoNamedTest2();

        [Test] public void TestDuplicateProperties() => DoNamedTest2();

        // A hole named after a complex expression is left out of the naming rules but still counts
        // as a duplicate
        [Test] public void TestDuplicatePropertiesFromExpressions() => DoNamedTest2();

        [Test] public void TestMessageIsSentence() => DoNamedTest2();

        [Test] public void TestAllLogLevels() => DoNamedTest2();

        // An interpolated string is not a compile time constant, but a ZLogger 2.x template is meant to
        // be one: the handler consumes the holes, so the template must not be reported
        [Test] public void TestTemplateIsCompileTimeConstant() => DoNamedTest2();

        // A hole with no ':@name' is named after the source text of its expression, so a name such as
        // 'uri.Host' is left out of the naming rules, which could not suggest a rename for it
        [Test] public void TestComplexExpressionNotChecked() => DoNamedTest2();

        // '{1}' is named after its expression and therefore looks positional, but an interpolated
        // template has no positional holes to report
        [Test] public void TestPositionalPropertyNotReported() => DoNamedTest2();

        // The parameters after the template hold no hole values, so an exception passed as the `context`
        // argument is not an exception passed as a template argument
        [Test] public void TestExceptionInContextArgument() => DoNamedTest2();

        [Test] public void TestExceptionInTemplateHole() => DoNamedTest2();

        [Test] public void TestExceptionOverload() => DoNamedTest2();

        // Destructuring is Serilog syntax; ZLogger serializes with ':json' instead, so the destructuring
        // analyzers stay out of interpolated templates
        [Test] public void TestComplexObjectNoDestructuring() => DoNamedTest2();
    }
}
