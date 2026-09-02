using Microsoft.Extensions.Logging;

class Outer
{
	public class Inner
	{
		ILogger<B> _log;

		public Inner(ILogger<{caret}B> log)
		{
			_log = log;
		}
	}
}

class B { }
