using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            var name = "world";
            Log.Logger.Information("Greeting {Name}", name);
        }
    }
}
