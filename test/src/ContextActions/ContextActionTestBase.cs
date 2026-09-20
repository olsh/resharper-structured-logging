using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using JetBrains.ReSharper.TestFramework;

using NUnit.Framework;

using ReSharper.Structured.Logging.Tests.Constants;

namespace ReSharper.Structured.Logging.Tests.ContextActions
{
    // The [LoggerMessage] attribute and the Microsoft.Extensions.Logging extension methods only resolve on a
    // modern target framework
    [TestFixture]
    [TestNet60]
    [TestPackages(NugetPackages.MicrosoftLoggingPackage, Inherits = true)]

    // ReSharper disable once TestClassNameSuffixWarning
    public abstract class ContextActionTestBase<TContextAction> : CSharpContextActionExecuteTestBase<TContextAction>
        where TContextAction : class, IContextAction
    {
        protected override string ExtraPath => SubPath;

        protected override string RelativeTestDataPath => @"ContextActions\" + SubPath;

        protected abstract string SubPath { get; }
    }
}
