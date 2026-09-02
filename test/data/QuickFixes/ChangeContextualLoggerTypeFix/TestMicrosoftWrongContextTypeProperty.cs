using Microsoft.Extensions.Logging;

class A
{
	ILogger<B> Log { get; }

	public A(ILogger<{caret}B> log)
	{
		Log = log;
	}
}

class B { }
