using Microsoft.Extensions.Logging;

namespace X
{
	class A { }
}

namespace Y
{
	class A
	{
		ILogger<X.A> _log;
		
		public A(ILogger<X.A> log)
		{
			_log = log;
		}
	}
}
