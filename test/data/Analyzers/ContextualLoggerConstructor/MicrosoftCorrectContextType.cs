using Microsoft.Extensions.Logging;

class A
{
	ILogger<A> _log;
	
	public A(ILogger<A> log)
	{
		_log = log;
	}
}
