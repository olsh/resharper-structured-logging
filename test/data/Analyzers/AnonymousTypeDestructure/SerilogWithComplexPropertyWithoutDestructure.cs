using Serilog;
using System;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            Log.Logger.Information("{MyProperty}", new { Test = 1, Complex = new Random() });
        }
    }
}
