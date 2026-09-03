using NUnit.Framework;

using ReSharper.Structured.Logging.QuickFixes;

namespace ReSharper.Structured.Logging.Tests.QuickFixes
{
    public class DestructureContextLogPropertyFixTests : QuickFixTestBase<DestructureContextLogPropertyFix>
    {
        protected override string SubPath => "DestructureContextLogPropertyFix";

        [Test] public void TestSerilogContextComplexObject() => DoNamedTest();

        // The flag is appended after the last argument rather than spliced into the list
        [Test] public void TestSerilogContextNamedArguments() => DoNamedTest();
    }
}
