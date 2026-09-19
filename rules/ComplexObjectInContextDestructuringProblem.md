#### Complex objects with default `ToString()` implementation probably need to be destructured

Noncompliant Code Example:
```csharp
class User
{
    public int Age { get; set; }
}

...

LogContext.PushProperty("User", new User());
logger.ForContext("User", new User());
new LoggerConfiguration().Enrich.WithProperty("User", new User());
```

Compliant Solution:
```csharp
class User
{
    public int Age { get; set; }
}

...

LogContext.PushProperty("User", new User(), true);
logger.ForContext("User", new User(), true);
new LoggerConfiguration().Enrich.WithProperty("User", new User(), true);

// or

LogContext.PushProperty("User", new User(), false);
logger.ForContext("User", new User(), false);
new LoggerConfiguration().Enrich.WithProperty("User", new User(), false);
```

Serilog only: `LogContext.PushProperty`, `ForContext(string, object)` and `Enrich.WithProperty` all take the optional `destructureObjects` flag, while NLog's `ScopeContext.PushProperty` has no such flag to set, so it is not reported.
