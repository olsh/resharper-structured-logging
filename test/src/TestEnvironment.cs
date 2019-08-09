using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;

using NUnit.Framework;

#if RIDER
using JetBrains.ReSharper.Host.Env;
#endif

[assembly: RequiresThread(System.Threading.ApartmentState.STA)]

namespace ReSharper.Structured.Logging.Tests
{
    [ZoneDefinition]
    public interface IReSharperSerilog : ITestsEnvZone, IRequire<PsiFeatureTestZone>
#if RIDER
                                         , IRequire<IRiderPlatformZone>
#endif
    {
    }

    [SetUpFixture]
    public class TestEnvironment : ExtensionTestEnvironmentAssembly<IReSharperSerilog>
    {
    }
}
