using System;
using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            Log.Logger.Error(new MyException(), "{MyProperty}", new Random());
        }
    }

    public class MyException : Exception
    {
    }
}
