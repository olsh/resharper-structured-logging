using System.Collections.Generic;
using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(List<int> ids)
        {
            Log.Logger.Information("Ids {@Ids}", ids);
        }
    }
}
