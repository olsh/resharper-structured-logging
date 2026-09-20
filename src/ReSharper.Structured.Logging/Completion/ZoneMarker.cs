using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Feature.Services;
using JetBrains.ReSharper.Psi.CSharp;

namespace ReSharper.Structured.Logging.Completion;

/// <summary>
/// The completion infrastructure lives behind its own zone, which the plugin root marker does not ask
/// for. Requiring it here rather than there keeps the analyzers available wherever they are today.
/// </summary>
[ZoneMarker]
public class ZoneMarker : IRequire<ILanguageCSharpZone>, IRequire<ICodeEditingZone>
{
}
