using Microsoft.Extensions.Logging;

static class A
{
    public static ILogger<T> Create<T>(ILoggerFactory loggerFactory)
    {
        return loggerFactory.CreateLogger<T>();
    }
}
