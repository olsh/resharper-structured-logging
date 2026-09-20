using Serilog;

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
        public static void Main(Order order, Customer customer)
        {
            Log.Logger.Information("Shipped " + "{{caret}", order.Id);
        }
    }
}
