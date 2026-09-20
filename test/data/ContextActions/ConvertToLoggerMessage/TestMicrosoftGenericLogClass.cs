using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    internal static partial class Log<T>
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Cache warmed up")]
        public static partial void CacheWarmedUp(ILogger logger);
    }

    public static class Program
    {
        public static void Main(ILogger logger, int orderId)
        {
            logger.{caret}LogInformation("Order {OrderId} shipped", orderId);
        }
    }
}
