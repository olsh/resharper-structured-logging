using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class Log
    {
        public static void Write(string text)
        {
        }
    }

    public static class Program
    {
        public static void Main(ILogger logger, int orderId)
        {
            logger.{caret}LogInformation("Order {OrderId} shipped", orderId);
        }
    }
}
