using System;
using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(ILogger logger, Exception exception, int orderId)
        {
            logger.{caret}LogError(exception, "Order {OrderId} failed to ship", orderId);
        }
    }
}
