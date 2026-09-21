using JetBrains.Annotations;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.ReSharper.FeaturesTestFramework.Completion;
using JetBrains.ReSharper.TestFramework;

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
    }
}
