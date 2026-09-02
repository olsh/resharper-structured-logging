using Microsoft.Extensions.Logging;

struct A(ILogger<{caret}B> log)
{
	private readonly ILogger<B> _log = log;
}

class B { }
