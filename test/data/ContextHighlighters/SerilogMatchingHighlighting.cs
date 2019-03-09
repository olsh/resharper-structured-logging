using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            Log.Logger.Information("{MyProperty{caret:a}}", 1);
            Log.Logger.Information("{MyProperty}", {caret:b}1);
        }
    }
}
