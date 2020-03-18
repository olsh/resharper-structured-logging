using JetBrains.ReSharper.Feature.Services.Daemon;

using ReSharper.Structured.Logging.Settings;

[assembly: RegisterConfigurableHighlightingsGroup(StructuredLoggingGroup.Id, StructuredLoggingGroup.Name)]

namespace ReSharper.Structured.Logging.Settings
{
    public class StructuredLoggingGroup
    {
        public const string Id = "StructuredLogging";

        public const string Name = "Structured Logging Misuse";
    }
}
