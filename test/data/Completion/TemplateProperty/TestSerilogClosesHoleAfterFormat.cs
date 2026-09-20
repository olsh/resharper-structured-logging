// ${COMPLETE_ITEM:OrderId}
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
            Log.Logger.Information("Shipped {Ord{caret}:D4", order.Id);
        }
    }
}
