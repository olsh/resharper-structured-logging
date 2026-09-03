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

        [Test] public void TestMicrosoftWrongContextType() => DoNamedTest();

        [Test] public void TestMicrosoftWrongContextTypeMultipleParameters() => DoNamedTest();

        [Test] public void TestMicrosoftWrongContextTypeMultipleNamespaces() => DoNamedTest();

        [Test] public void TestMicrosoftWrongContextTypeWithoutField() => DoNamedTest();

        [Test] public void TestMicrosoftWrongContextTypeProperty() => DoNamedTest();

        [Test] public void TestMicrosoftWrongContextTypeGenericClass() => DoNamedTest();

        [Test] public void TestMicrosoftWrongContextTypeNestedClass() => DoNamedTest();

        [Test] public void TestMicrosoftExpressionBodiedConstructor() => DoNamedTest();

        [Test] public void TestMicrosoftCovariantMemberIsNotChanged() => DoNamedTest();

        [Test] public void TestMicrosoftFactoryWrongContextType() => DoNamedTest();

        [Test] public void TestMicrosoftFactoryGenericMemberIsChanged() => DoNamedTest();

        [Test] public void TestMicrosoftFactoryCovariantMemberIsNotChanged() => DoNamedTest();

        [Test] public void TestMicrosoftFactoryLocalVariable() => DoNamedTest();

        [Test] public void TestMicrosoftFactoryImplicitlyTypedLocalVariable() => DoNamedTest();
    }
}
