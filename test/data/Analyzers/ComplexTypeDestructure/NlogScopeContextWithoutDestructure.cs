using System;
using NLog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            using (ScopeContext.PushProperty("Test", new Random()))
            {
            }
        }
    }
}
