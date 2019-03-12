using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            Log.Logger.Information("{MyProperty} {AnotherProperty}", 1);
            Log.Logger.Information("{MyProperty}", 1, 2);
            Log.Logger.Information("Plain string", 1, 2);
        }
    }
}
