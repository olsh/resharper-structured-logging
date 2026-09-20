using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public class OrderService
    {
        private readonly ILogger<OrderService> _logger;

        public OrderService(ILogger<OrderService> logger)
        {
            _logger = logger;
        }

        public void Ship(int orderId)
        {
            _logger.{caret}LogInformation("Order {OrderId} shipped", orderId);
        }
    }
}
