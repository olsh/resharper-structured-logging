using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(ILogger logger, EventId eventId, int orderId)
        {
            logger.{off}LogInformation(eventId, "Order {OrderId} shipped", orderId);
        }
    }
}
