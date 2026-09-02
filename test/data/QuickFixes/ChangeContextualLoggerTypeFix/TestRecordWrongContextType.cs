using Microsoft.Extensions.Logging;

record A(ILogger<{caret}B> Log);

class B { }
