using Microsoft.Extensions.Logging;

class A(ILogger<A> log)
{
	private readonly ILogger<A> _log = log;
}
