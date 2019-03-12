using NLog.Fluent;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
        	var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info()
                .Message("Test {MyProperpty}", 1);
        }
    }
}
