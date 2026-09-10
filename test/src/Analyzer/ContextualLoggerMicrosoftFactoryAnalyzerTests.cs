using JetBrains.ReSharper.TestFramework;

using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    [TestNet60]
    public class ContextualLoggerMicrosoftFactoryAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "ContextualLoggerMicrosoftFactory";

        [Test] public void TestMicrosoftCorrectContextType() => DoNamedTest2();

        [Test] public void TestMicrosoftWrongContextType() => DoNamedTest2();

        [Test] public void TestMicrosoftGenericTypeParameterIsIgnored() => DoNamedTest2();

        [Test] public void TestMicrosoftLoggerPassedToConstructorIsIgnored() => DoNamedTest2();

        [Test] public void TestMicrosoftReturnedLoggerIsIgnored() => DoNamedTest2();

        [Test] public void TestMicrosoftExpressionBodiedFactoryIsIgnored() => DoNamedTest2();

        [Test] public void TestMicrosoftLoggerAssignedToOtherTypeIsIgnored() => DoNamedTest2();

        [Test] public void TestMicrosoftLoggerInObjectInitializerIsIgnored() => DoNamedTest2();

        [Test] public void TestMicrosoftEscapingLocalVariableIsIgnored() => DoNamedTest2();

        [Test] public void TestMicrosoftLocalVariableIsReported() => DoNamedTest2();

        [Test] public void TestMicrosoftUsedLocalVariableIsReported() => DoNamedTest2();

        [Test] public void TestMicrosoftReassignedLocalVariableIsReported() => DoNamedTest2();

        [Test] public void TestMicrosoftReassignedEscapingLocalVariableIsIgnored() => DoNamedTest2();
    }
}
