using Microsoft.Extensions.Logging;

class A
{
    ILogger<object> _log;

    public A(ILoggerFactory loggerFactory)
    {
        _log = loggerFactory.CreateLogger<{caret}B>();
    }
}

class B { }
