﻿using JetBrains.ReSharper.TestFramework;

using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    [TestNet60]
    public class ContextualLoggerConstructorAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "ContextualLoggerConstructor";

        [Test] public void TestMicrosoftCorrectContextType() => DoNamedTest2();

        [Test] public void TestMicrosoftWrongContextType() => DoNamedTest2();

        [Test] public void TestMicrosoftWrongContextTypeMultipleNamespaces() => DoNamedTest2();

        [Test] public void TestMicrosoftWrongContextTypeMultipleParameters() => DoNamedTest2();
    }
}
