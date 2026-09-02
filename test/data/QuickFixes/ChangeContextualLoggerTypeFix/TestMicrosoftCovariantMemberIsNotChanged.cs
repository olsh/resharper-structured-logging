using Microsoft.Extensions.Logging;

class A
{
	ILogger<object> _log;

	public A(ILogger<{caret}B> log)
	{
		_log = log;
	}
}

class B { }
