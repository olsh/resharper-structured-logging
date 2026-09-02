using Microsoft.Extensions.Logging;

class A
{
	public A(ILogger<{caret}B> log)
	{
	}
}

class B { }
