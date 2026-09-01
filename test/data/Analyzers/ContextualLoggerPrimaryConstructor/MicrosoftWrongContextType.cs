using Microsoft.Extensions.Logging;

class A(ILogger<B> log)
{
	private readonly ILogger<B> _log = log;
}

class B { }
