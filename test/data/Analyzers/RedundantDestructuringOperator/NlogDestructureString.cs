using NLog;

namespace ConsoleApp
{
    public static class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Main(string userName)
        {
            Log.Info("User {@UserName} logged in", userName);
        }
    }
}
