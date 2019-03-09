using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;

using NUnit.Framework;

[assembly: RequiresSTA]

namespace ReSharper.Structured.Logging.Tests
{
    [ZoneDefinition]
    public interface IReSharperSerilog : ITestsEnvZone, IRequire<PsiFeatureTestZone>
    {
    }

    [SetUpFixture]
    public class TestEnvironment : ExtensionTestEnvironmentAssembly<IReSharperSerilog>
    {
    }
}
