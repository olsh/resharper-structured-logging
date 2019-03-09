using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon.StringAnalysis;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.FeaturesTestFramework.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TestFramework;

using NUnit.Framework;

using ReSharper.Structured.Logging.Highlighting;
using ReSharper.Structured.Logging.Tests.Constants;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    [TestPackages(NugetPackages.SerilogNugetPackage, NugetPackages.MicrosoftLoggingPackage, Inherits = true)]
    public class TemplateFormatAnalyzerTests : CSharpHighlightingTestBase
    {
        protected override string RelativeTestDataPath => "Analyzers";

        [Test]
        public void TestSerilogValidNamedProperty()
        {
            DoNamedTest2();
        }

        [Test]
        public void TestSerilogInValidNamedProperty()
        {
            DoNamedTest2();
        }

        [Test]
        public void TestSerilogInValidPositionProperty()
        {
            DoNamedTest2();
        }

        [Test]
        public void TestSerilogValidPositionProperty()
        {
            DoNamedTest2();
        }

        protected override bool HighlightingPredicate(
            IHighlighting highlighting,
            IPsiSourceFile sourceFile,
            IContextBoundSettingsStore settingsStore)
        {
            return highlighting is TemplateFormatStringArgumentIsNotUsedWarning
                   || highlighting is TemplateFormatStringInexistingArgumentWarning
                   || highlighting is StringEscapeCharacterHighlighting;
        }
    }
}
