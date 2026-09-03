using NLog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            using (ScopeContext.PushProperty("test", 1))
            {
            }

            using (ScopeContext.PushProperty("myProperty", (object)1))
            {
            }
        }
    }
}
