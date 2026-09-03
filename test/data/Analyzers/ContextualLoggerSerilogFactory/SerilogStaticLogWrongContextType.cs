using Serilog;

class A
{
    private static readonly ILogger Logger = Log.ForContext<B>();
}

class B { }
