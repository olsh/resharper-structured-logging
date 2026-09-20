using System;

using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class LogMessages
    {
        private static readonly Action<ILogger, int, Exception> Shipped =
            LoggerMessage.Define<int>(LogLevel.Information, new EventId(0), "Shipped {{caret}");
    }
}
