using Microsoft.Extensions.Logging;

record A(ILogger<B> Log);

class B { }
