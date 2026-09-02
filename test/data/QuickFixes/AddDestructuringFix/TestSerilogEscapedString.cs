using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            Log.Logger.Information("Escaped \r\n {MyPro{caret}perty} \r\n string", new { Test = 1 });
        }
    }
}
