using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon.StringAnalysis;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.FeaturesTestFramework.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TestFramework;

using ReSharper.Structured.Logging.Highlighting;
using ReSharper.Structured.Logging.Tests.Constants;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    [TestPackages(NugetPackages.SerilogNugetPackage, NugetPackages.MicrosoftLoggingPackage, NugetPackages.NlogLoggingPackage, Inherits = true)]
    public class MessageTemplateAnalyzerTestBase : CSharpHighlightingTestBase
    {
        protected override string RelativeTestDataPath => "Analyzers";

        protected override bool HighlightingPredicate(
            IHighlighting highlighting,
            IPsiSourceFile sourceFile,
            IContextBoundSettingsStore settingsStore)
        {
            return highlighting is TemplateFormatStringArgumentIsNotUsedWarning
                   || highlighting is TemplateFormatStringInexistingArgumentWarning
                   || highlighting is StringEscapeCharacterHighlighting
                   || highlighting is DuplicateTemplatePropertyWarning
                   || highlighting is ExceptionPassedAsTemplateArgumentWarning;
        }
    }
}
