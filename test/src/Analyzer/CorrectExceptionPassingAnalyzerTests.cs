using NUnit.Framework;

namespace ReSharper.Structured.Logging.Tests.Analyzer
{
    public class CorrectExceptionPassingAnalyzerTests : MessageTemplateAnalyzerTestBase
    {
        protected override string SubPath => "CorrectExceptionPassing";

        [Test]
        public void TestSerilogCorrectExceptionPassing() => DoNamedTest2();

        [Test]
        public void TestSerilogIncorrectExceptionPassing() => DoNamedTest2();

        [Test]
        public void TestSerilogIncorrectExceptionPassingDynamicTemplate() => DoNamedTest2();

        [Test]
        public void TestSerilogMultipleExceptionPassing() => DoNamedTest2();

        [Test]
        public void TestSerilogNamedArgumentException() => DoNamedTest2();

        [Test]
        public void TestSerilogExceptionInArrayArguments() => DoNamedTest2();

        [Test]
        public void TestSerilogExceptionMessageAsTemplateArgument() => DoNamedTest2();

        [Test]
        public void TestSerilogExceptionToStringAsTemplateArgument() => DoNamedTest2();

        [Test]
        public void TestSerilogExceptionStackTraceAsTemplateArgument() => DoNamedTest2();

        [Test]
        public void TestSerilogBaseExceptionMessageAsTemplateArgument() => DoNamedTest2();

        // Data, HResult and the properties of a derived exception are scalars worth logging on their own
        [Test]
        public void TestSerilogExceptionScalarPropertiesNotReported() => DoNamedTest2();

        // The exception is not lost, so the repeated text is the author's choice to make
        [Test]
        public void TestSerilogExceptionMessageWithOccupiedExceptionArgument() => DoNamedTest2();

        [Test]
        public void TestSerilogNonExceptionMessagePropertyNotReported() => DoNamedTest2();
    }
}
