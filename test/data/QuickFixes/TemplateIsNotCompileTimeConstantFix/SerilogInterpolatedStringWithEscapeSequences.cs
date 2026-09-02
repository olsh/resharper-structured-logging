using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            var aaa = 123;
            var bbb = 456;

            Log.Logger.Information($"A: \"{aa{caret}a}\"\tB: {bbb}\n");
        }
    }
}
