using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            Log.Logger.Information("{0} {1}", 1);
            Log.Logger.Information("{1}", 1, 2);
        }
    }
}
