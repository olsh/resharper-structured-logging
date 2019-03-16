using Microsoft.Extensions.Logging;

class A
{
	ILogger<B> _log;
	
	public A(ILogger<B> log)
	{
		_log = log;
	}
}

class B { }
