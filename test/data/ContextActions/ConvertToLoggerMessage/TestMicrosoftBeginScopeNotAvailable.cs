using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(ILogger logger, int orderId)
        {
            using (logger.{off}BeginScope("Order {OrderId}", orderId))
            {
            }
        }
    }
}
