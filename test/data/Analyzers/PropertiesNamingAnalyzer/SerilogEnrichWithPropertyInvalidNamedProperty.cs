using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            new LoggerConfiguration().Enrich.WithProperty("test", 1);
        }
    }
}
