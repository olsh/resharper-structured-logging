using System;
using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(Exception exception)
        {
            Log.Logger.Error("Import failed {Error}", exception.Message);
        }
    }
}
