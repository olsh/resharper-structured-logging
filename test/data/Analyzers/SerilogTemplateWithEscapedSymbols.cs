using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            Log.Logger.Information("\r\n{MyProperty} \r\n {OtherProperty}", 1, 2);
        }
    }
}
