using System;
using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            Log.Logger.Information("{MyProperty}", new B());
        }
    }

    public class A
    {
        public override string ToString() => "Custom ToString";
    }

    public class B: A { }
}
