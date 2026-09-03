using NLog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            using (ScopeContext.PushProperty("{caret}test", 1))
            {
            }
        }
    }
}
