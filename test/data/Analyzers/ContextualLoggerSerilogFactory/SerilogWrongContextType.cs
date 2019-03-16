using Serilog;

class A
{
    private static readonly ILogger Logger = Logger.ForContext<B>();
}

class B {} 
