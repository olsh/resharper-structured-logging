using System.IO;
using System.Linq;

using JetBrains.Annotations;
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
    [TestPackages(
        NugetPackages.SerilogNugetPackage,
        NugetPackages.MicrosoftLoggingPackage,
        NugetPackages.NlogLoggingPackage,
        Inherits = true)]
    public abstract class MessageTemplateAnalyzerTestBase : CSharpHighlightingTestBase
    {
        protected abstract string SubPath { get; }

        protected override string RelativeTestDataPath => @"Analyzers\" + SubPath;

        [TestCaseSource(nameof(FileNames))]
        public void Test(string fileName)
        {
            DoTestSolution(fileName);
        }

        protected override void DoTestSolution([NotNull] params string[] fileSet)
        {
            ExecuteWithinSettingsTransaction(
                settingsStore =>
                    {
                        RunGuarded(() => { });
                        base.DoTestSolution(fileSet);
                    });
        }

        // ReSharper disable once MemberCanBePrivate.Global
        protected TestCaseData[] FileNames()
        {
            return Directory.GetFiles(@"..\..\..\..\test\data\" + RelativeTestDataPath, "*.cs")
                .Select(x => new TestCaseData(Path.GetFileName(x)))
                .ToArray();
        }

        protected override bool HighlightingPredicate(
            IHighlighting highlighting,
            IPsiSourceFile sourceFile,
            IContextBoundSettingsStore settingsStore)
        {
            return highlighting is TemplateFormatStringArgumentIsNotUsedWarning
                   || highlighting is TemplateFormatStringUnexistingArgumentWarning
                   || highlighting is StringEscapeCharacterHighlighting
                   || highlighting is DuplicateTemplatePropertyWarning
                   || highlighting is AnonymousObjectDestructuringWarning
                   || highlighting is ExceptionPassedAsTemplateArgumentWarning;
        }
    }
}
