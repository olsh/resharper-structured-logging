using Microsoft.Extensions.Logging;

class A
{
    ILogger _log;

    public A(ILoggerFactory loggerFactory)
    {
        _log = loggerFactory.CreateLogger<{caret}B>();
    }
}

class B { }
