using System;

using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public class Endpoint
    {
        public string Host { get; set; }
    }

    public static class LogMessages
    {
        private static readonly Action<ILogger, Endpoint, Exception> CouldNotOpenSocket =
            LoggerMessage.Define<Endpoint>(LogLevel.Information, new EventId(0), "Could not open socket to {@Endpoint}");
    }
}
