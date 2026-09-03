using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(int quota)
        {
            Log.Logger.Information("Exceeded {0{caret}}", quota);
        }
    }
}
