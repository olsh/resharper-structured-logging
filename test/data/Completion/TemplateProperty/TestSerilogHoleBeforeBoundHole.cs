using Serilog;

namespace ConsoleApp
{
    public class Address
    {
        public string Country { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
    }

    public static class Program
    {
        public static void Main(Order order, Address address)
        {
            Log.Logger.Information("{{caret}} shipped to {Country}", order.Id, address.Country);
        }
    }
}
