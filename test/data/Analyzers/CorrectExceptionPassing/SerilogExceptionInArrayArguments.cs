using System;
using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            Log.Logger.Information("{One} {Two}", new object[] { 1, new Exception() });
        }
    }
}
