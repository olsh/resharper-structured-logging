using System.IO;
using System.Linq;

using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using JetBrains.ReSharper.TestFramework;

using NUnit.Framework;

using ReSharper.Structured.Logging.Tests.Constants;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    [TestFixture]
    [TestNetFramework46]
    [TestPackages(
        NugetPackages.SerilogNugetPackage,
        NugetPackages.MicrosoftLoggingPackage,
        NugetPackages.NlogLoggingPackage,
        Inherits = true)]

    // ReSharper disable once TestClassNameSuffixWarning
    public abstract class QuickFixTestBase<TQuickFix> : CSharpQuickFixTestBase<TQuickFix>
        where TQuickFix : IQuickFix
    {
        protected override string RelativeTestDataPath => @"QuickFixes\" + SubPath;

        protected abstract string SubPath { get; }
    }
}
