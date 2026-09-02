using Serilog;
using System;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            Log.Logger.Information("{MyPro{caret}perty}", new Random());
        }
    }
}
