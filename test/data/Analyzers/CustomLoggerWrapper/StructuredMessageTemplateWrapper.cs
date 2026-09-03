using System;

namespace ConsoleApp
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class StructuredMessageTemplateAttribute : Attribute
    {
    }

    public static class LoggerExtensions
    {
        public static void LogInformation(
            [StructuredMessageTemplate] string messageTemplate,
            params object[] propertyValues)
        {
        }
    }

    public static class Program
    {
        public static void Main()
        {
            LoggerExtensions.LogInformation("{0}", 1);
        }
    }
}
