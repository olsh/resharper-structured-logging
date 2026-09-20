using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public class Customer
    {
        public string Email { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
    }

    public static class Program
    {
        public static void Main(ILogger logger, Order order, Customer customer)
        {
            logger.LogInformation("Shipped {{caret}", order.Id, customer.Email);
        }
    }
}
