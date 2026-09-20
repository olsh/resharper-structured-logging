using Serilog;

namespace ConsoleApp
{
    public class Customer
    {
        public string Name { get; set; }
    }

    public class Order
    {
        public Customer Customer { get; set; }
    }

    public static class Program
    {
        public static void Main(Order order)
        {
            Log.Logger.Information("Shipped to {{caret}", order.Customer.Name);
        }
    }
}
