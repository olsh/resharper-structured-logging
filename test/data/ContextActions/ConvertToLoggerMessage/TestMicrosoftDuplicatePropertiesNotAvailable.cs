using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(ILogger logger, int orderId, int otherOrderId)
        {
            logger.{off}LogInformation("Order {OrderId} replaced {OrderId}", orderId, otherOrderId);
        }
    }
}
