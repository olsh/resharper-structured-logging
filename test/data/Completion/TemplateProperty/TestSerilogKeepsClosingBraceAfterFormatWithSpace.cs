// ${COMPLETE_ITEM:Timestamp}
using System;

using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(DateTime timestamp)
        {
            Log.Logger.Information("Started at {{caret}:yyyy MM dd}", timestamp);
        }
    }
}
