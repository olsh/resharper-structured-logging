using Serilog;

class A
{
    private static readonly ILogger ContextLogger = Log.Logger.ForContext<{caret}B>();
}

class B { }
