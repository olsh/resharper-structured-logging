using JetBrains.Annotations;
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
    [TestPackages(
        NugetPackages.SerilogNugetPackage,
        NugetPackages.MicrosoftLoggingPackage,
        NugetPackages.NlogLoggingPackage,
        Inherits = true)]
    public abstract class MessageTemplateAnalyzerTestBase : CSharpHighlightingTestBase
    {
        protected abstract string SubPath { get; }

        protected override string RelativeTestDataPath => @"Analyzers\" + SubPath;

        protected override void DoTestSolution([NotNull] params string[] fileSet)
        {
            ExecuteWithinSettingsTransaction(
                settingsStore =>
                    {
                        RunGuarded(() => MutateSettings(settingsStore));
                        base.DoTestSolution(fileSet);
                    });
        }

        protected override bool HighlightingPredicate(
            IHighlighting highlighting,
            IPsiSourceFile sourceFile,
            IContextBoundSettingsStore settingsStore)
        {
            return highlighting is TemplateFormatStringNonExistingArgumentWarning
                   || highlighting is StringEscapeCharacterHighlighting
                   || highlighting is DuplicateTemplatePropertyWarning
                   || highlighting is AnonymousObjectDestructuringWarning
                   || highlighting is ContextualLoggerWarning
                   || highlighting is ExceptionPassedAsTemplateArgumentWarning
                   || highlighting is ComplexObjectDestructuringWarning
                   || highlighting is ComplexObjectDestructuringInContextWarning
                   || highlighting is PositionalPropertyUsedWarning
                   || highlighting is InconsistentLogPropertyNamingWarning
                   || highlighting is InconsistentContextLogPropertyNamingWarning
                   || highlighting is LogMessageIsSentenceWarning;
        }

        protected virtual void MutateSettings([NotNull] IContextBoundSettingsStore settingsStore)
        {
        }
    }
}
