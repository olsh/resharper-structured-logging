using Serilog;

namespace ConsoleApp
{
    public class Order
    {
        public int Id { get; set; }
    }

    public static class Program
    {
        public static void Main(Order order)
        {
            Log.Logger.Information("Order {0{caret}} processed", order.Id);
        }
    }
}
