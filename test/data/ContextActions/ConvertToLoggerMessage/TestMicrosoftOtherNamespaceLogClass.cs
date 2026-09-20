using Microsoft.Extensions.Logging;

namespace Other
{
    internal static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Cache warmed up")]
        public static partial void CacheWarmedUp(ILogger logger);
    }
}

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(ILogger logger, int orderId)
        {
            logger.{caret}LogInformation("Order {OrderId} shipped", orderId);
        }
    }
}
