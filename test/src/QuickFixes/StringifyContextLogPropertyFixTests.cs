using NUnit.Framework;

using ReSharper.Structured.Logging.QuickFixes;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    public class StringifyContextLogPropertyFixTests : QuickFixTestBase<StringifyContextLogPropertyFix>
    {
        protected override string SubPath => "StringifyContextLogPropertyFix";

        [Test] public void TestSerilogContextComplexObject() => DoNamedTest();

        // The flag is appended after the last argument rather than spliced into the list
        [Test] public void TestSerilogContextNamedArguments() => DoNamedTest();
    }
}
