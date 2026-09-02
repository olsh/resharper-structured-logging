using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using JetBrains.ReSharper.TestFramework;

using NUnit.Framework;

using ReSharper.Structured.Logging.QuickFixes;
using ReSharper.Structured.Logging.Tests.Constants;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    [TestFixture]
    [TestNet60]
    [TestPackages(NugetPackages.MicrosoftLoggingPackage)]
    public class ChangeContextualLoggerTypeFixDotNet6Tests : CSharpQuickFixTestBase<ChangeContextualLoggerTypeFix>
    {
        protected override string RelativeTestDataPath => @"QuickFixes\ChangeContextualLoggerTypeFix";

        [Test] public void TestMicrosoftWrongContextType() => DoNamedTest2();

        [Test] public void TestMicrosoftWrongContextTypeMultipleParameters() => DoNamedTest2();

        [Test] public void TestMicrosoftWrongContextTypeMultipleNamespaces() => DoNamedTest2();

        [Test] public void TestMicrosoftWrongContextTypeWithoutField() => DoNamedTest2();

        [Test] public void TestMicrosoftWrongContextTypeProperty() => DoNamedTest2();

        [Test] public void TestMicrosoftWrongContextTypeGenericClass() => DoNamedTest2();

        [Test] public void TestMicrosoftWrongContextTypeNestedClass() => DoNamedTest2();

        [Test] public void TestMicrosoftExpressionBodiedConstructor() => DoNamedTest2();

        [Test] public void TestMicrosoftCovariantMemberIsNotChanged() => DoNamedTest2();
    }
}
