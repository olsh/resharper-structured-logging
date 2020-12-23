using System.IO;

using JetBrains.Annotations;
using JetBrains.ReSharper.FeaturesTestFramework.ContextHighlighters;
using JetBrains.ReSharper.TestFramework;
using JetBrains.Util;

using NUnit.Framework;

using ReSharper.Structured.Logging.Tests.Constants;

namespace ReSharper.Structured.Logging.Tests.Highlighting
{
    [TestPackages(NugetPackages.SerilogNugetPackage, NugetPackages.MicrosoftLoggingPackage, NugetPackages.NlogLoggingPackage, Inherits = true)]
    [TestNetFramework46]
    public class TemplateFormatItemAndMatchingArgumentHighlighterTests : ContextHighlighterTestBase
    {
        protected override string ExtraPath => string.Empty;

        [Test] public void TestSerilogMatchingHighlighting() => DoNamedTest2();

        [Test] public void TestNlogMatchingHighlighting() => DoNamedTest2();

        [Test] public void TestSerilogMethodArgumentMatchingHighlighting() => DoNamedTest2();

        protected override CaretPositionsProcessor CreateCaretPositionProcessor(FileSystemPath temporaryDirectory)
        {
            // ReSharper disable once ExceptionNotDocumented
            return new StructuredLoggingCaretPositionsProcessor(FileSystemPath.TryParse(Path.GetTempPath()));
        }

        private class StructuredLoggingCaretPositionsProcessor : CaretPositionsProcessor
        {
            public StructuredLoggingCaretPositionsProcessor([NotNull] FileSystemPath temporaryDirectory)
                : base(temporaryDirectory)
            {
            }

            protected override bool IsValidPositionName(string name)
            {
                return name.Contains("caret");
            }
        }
    }
}
