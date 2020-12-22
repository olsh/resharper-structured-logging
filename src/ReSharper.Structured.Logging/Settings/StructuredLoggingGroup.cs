using JetBrains.ReSharper.Feature.Services.Daemon;

namespace ReSharper.Structured.Logging.Settings
{
    [RegisterConfigurableHighlightingsGroup(Id, Name)]
    public class StructuredLoggingGroup
    {
        public const string Id = "StructuredLogging";

        public const string Name = "Structured Logging Misuse";
    }
}
