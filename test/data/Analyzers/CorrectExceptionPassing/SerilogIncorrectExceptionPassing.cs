using System;
using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            Log.Logger.Information("{One} {Exc}", 1, new Exception());
        }
    }
}
