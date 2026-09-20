using System;
using Serilog;
using Serilog.Core.Enrichers;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            Log.ForContext(new PropertyEnricher("A", new Random()), new PropertyEnricher("B", new Random()));
            Log.ForContext<Program>();
            Log.ForContext(typeof(Program));
        }
    }
}
