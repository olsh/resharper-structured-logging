using System;
using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
        	int? a = 1;
            Log.Logger.Information("{$MyProperty}", a);
        }
    }
}
