using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            Log.Logger.Information(@"This {MyProperty} \r\n"
            	+ " \r\n + {OtherProperty} and \r\n"
                + @" \r\n + {OneMore} last \r\n", 1, 2);
        }
    }
}
