using System;
using System.IO;
using System.Text.RegularExpressions;

using JetBrains.Annotations;
using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.ReSharper.FeaturesTestFramework.Completion;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework.Utils;

using NUnit.Framework;

using ReSharper.Structured.Logging.Completion;
using ReSharper.Structured.Logging.Tests.Constants;

namespace ReSharper.Structured.Logging.Tests.Completion
{
    [TestFixture]
    [TestNet60]
    [TestPackages(
        NugetPackages.SerilogNugetPackage,
        NugetPackages.MicrosoftLoggingPackage,
        NugetPackages.NlogLoggingPackage,
        Inherits = true)]

    // ReSharper disable once TestClassNameSuffixWarning
    public abstract class TemplatePropertyCompletionTestBase : CodeCompletionTestBase
    {
        // Where the item was evaluated is one category in 2026.3 and two in 2026.2. None of that is
        // this extension to decide, so the gold records the categories around it instead. A wave that
        // renames another one fails on that wave alone, and the fix is to name it here as well
        private static readonly string[] WaveSpecificRelevanceCategories =
        {
            // 2026.2
            "FromSingleCompletion",
            "FromLightAndDynamicEvaluation",

            // 2026.3
            "FromLightEvaluation"
        };

        // The header 2026.3 added to the list dump. The stable wave writes no such line, so there is
        // nothing for it to be compared against
        private static readonly Regex RulesHeaderLine = new Regex(
            @"^Rules: .*\r?\n",
            RegexOptions.Multiline | RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(1));

        protected override string RelativeTestDataPath => @"Completion\" + SubPath;

        protected override CodeCompletionTestType TestType => CodeCompletionTestType.ModernList;

        // The order the names are offered in is what the nth hole rule is about, so the list is not
        // sorted alphabetically away from it
        protected override LookupListSorting Sorting => LookupListSorting.ByRelevance;

        protected abstract string SubPath { get; }

        /// <summary>
        /// The settings hook the analyzer fixtures use, on the method they hang it on: everything below
        /// this one already runs inside the reentrancy guard. CodeCompletionTestBase grew a hook of its
        /// own in 2026.3, but the extension is built for the stable wave as well, which has none.
        /// </summary>
        protected override void DoTestSolution([NotNull] params string[] fileSet)
        {
            ExecuteWithinSettingsTransaction(settingsStore =>
            {
                RunGuarded(() => MutateSettings(settingsStore));
                base.DoTestSolution(fileSet);
            });
        }

        protected virtual void MutateSettings([NotNull] IContextBoundSettingsStore settingsStore)
        {
        }

        /// <summary>
        /// Keeps the gold about this feature. Whatever else the engine offers inside a string literal is
        /// not what these tests assert, and it would turn every SDK bump into gold churn.
        /// </summary>
        protected override bool LookupItemFilter(ILookupItem lookupItem)
        {
            return lookupItem is TemplatePropertyLookupItem;
        }

        /// <summary>
        /// The same, for how the list itself is rendered. The names, their order and the range each one
        /// replaces are what this feature decides, and both waves agree on them; the framing around
        /// them changed in 2026.3, and the extension is released for the stable wave from this same
        /// source, so one gold has to read on both. CheckResultModernList writes the framing and is not
        /// virtual, which is why this happens at the writer.
        /// </summary>
        protected override TestFailureException ExecuteWithGold(IDocument document, Action<TextWriter> test)
        {
            // An action test writes the completed source rather than a list dump, so it has no framing
            // to drop and nothing below applies to it
            if (TestType != CodeCompletionTestType.ModernList)
            {
                return base.ExecuteWithGold(document, test);
            }

            return base.ExecuteWithGold(document, writer =>
            {
                var dump = new StringWriter { NewLine = writer.NewLine };
                test(dump);

                writer.Write(SharedAcrossWaves(dump.ToString()));
            });
        }

        private static string SharedAcrossWaves(string dump)
        {
            var shared = RulesHeaderLine.Replace(dump, string.Empty);

            // Only the relevance sort is presented in these terms. The bracketed lines under the
            // alphabetic one are placement groups, which both waves write the same way
            foreach (var category in WaveSpecificRelevanceCategories)
            {
                shared = shared
                    .Replace("[" + category + ", ", "[")
                    .Replace(", " + category + ",", ",")
                    .Replace(", " + category + "]", "]");
            }

            return shared;
        }
    }
}
