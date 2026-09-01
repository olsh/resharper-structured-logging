using Microsoft.Extensions.Logging;

class A(int a, ILogger<B> log)
{
	private readonly int _a = a;

	private readonly ILogger<B> _log = log;
}

class B { }
