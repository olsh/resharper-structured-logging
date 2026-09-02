using System;

using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class LogMessages
    {
        private static readonly Action<ILogger, string, Exception> CouldNotOpenSocket =
            LoggerMessage.Define<string>(LogLevel.Information, new EventId(0), "Could not open socket to {hostName}");
    }
}
