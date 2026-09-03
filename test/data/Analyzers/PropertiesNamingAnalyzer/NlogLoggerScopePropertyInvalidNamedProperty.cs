using NLog;

namespace ConsoleApp
{
    public static class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Main()
        {
            using (Log.PushScopeProperty("test", 1))
            {
            }
        }
    }
}
