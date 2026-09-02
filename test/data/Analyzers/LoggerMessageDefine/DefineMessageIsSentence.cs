using System;

using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class LogMessages
    {
        private static readonly Action<ILogger, string, Exception> Disconnected =
            LoggerMessage.Define<string>(LogLevel.Error, new EventId(3), "Disconnected from {Host}.");
    }
}
