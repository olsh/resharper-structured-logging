using Serilog;
using System;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            Log.Logger.Information("Test {MyProperty}", {caret:a}Math.Round(0d));
            Log.Logger.Information("Test {Hello} "
                + "\r\n {Another{caret:b}Property}", 1, Math.Round(0d));
        }
    }
}
