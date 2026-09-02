using Microsoft.Extensions.Logging;

class A
{
	ILogger<B> _log;

	public A(int a, ILogger<{caret}B> log)
	{
		_log = log;
	}
}

class B { }
