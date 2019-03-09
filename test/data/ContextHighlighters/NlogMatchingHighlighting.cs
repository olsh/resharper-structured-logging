namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();

            logger.Info("Hello World {test{caret:a}}", 1);
            logger.Info("Hello World {test}", {caret:b}1);
        }
    }
}
