using System;
using System.Threading.Tasks;

namespace ConsoleApp
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MessageTemplateFormatMethodAttribute : Attribute
    {
        public MessageTemplateFormatMethodAttribute(string messageTemplateParameterName)
        {
            MessageTemplateParameterName = messageTemplateParameterName;
        }

        public string MessageTemplateParameterName { get; }
    }

    public static class AsyncLog
    {
        [MessageTemplateFormatMethod("messageTemplate")]
        public static Task InformationAsync(string messageTemplate, params object[] propertyValues)
        {
            return Task.CompletedTask;
        }
    }

    public static class Program
    {
        public static async Task Run(string name)
        {
            await AsyncLog.InformationAsync("Greeting {Name}", name);
        }
    }
}
