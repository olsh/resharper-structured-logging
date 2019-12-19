using System;
using System.Collections.Generic;
using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
		public static void Main()
		{
			IEnumerable list = new List<string>() { "test" };
			Log.Logger.Information("{MyProperty}", list);
		}
    }
}
