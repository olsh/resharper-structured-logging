using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Log<T>(T value)
        {
            Serilog.Log.Logger.Information("Value {@Value}", value);
        }
    }
}
