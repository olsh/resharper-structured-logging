using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Psi.CSharp;

namespace ReSharper.Structured.Logging
{
    [ZoneMarker]
    public class ZoneMarker : IRequire<ILanguageCSharpZone>
    {
    }
}
