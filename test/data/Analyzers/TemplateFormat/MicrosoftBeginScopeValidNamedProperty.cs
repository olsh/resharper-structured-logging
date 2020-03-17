using Microsoft.Extensions.Logging;

class A
{
	public A(ILogger<A> log)
	{
		log.BeginScope("{MyProperty}", 1);
	}
}
