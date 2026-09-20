#### Destructuring or stringification operator has no effect on a scalar value

Serilog logs a string, a primitive, an enum, a `Guid`, a `DateTime`, a `DateTimeOffset`, a `TimeSpan` and a `Uri`
as a scalar whatever the operator says, and Microsoft.Extensions.Logging ignores the operator altogether.
The `@` or `$` is noise at best and, at worst, a sign that the author expected something to be expanded that will not be.

Noncompliant Code Example:
```csharp
Log.Information("User {@UserName} logged in from {@Ip}", user.Name, request.Ip);
Log.Information("Processed {@Count} items in {@Elapsed}", items.Count, stopwatch.Elapsed);
Log.Debug("Order {$OrderId}", order.Id);
```

Compliant Solution:
```csharp
Log.Information("User {UserName} logged in from {Ip}", user.Name, request.Ip);
Log.Information("Processed {Count} items in {Elapsed}", items.Count, stopwatch.Elapsed);
Log.Debug("Order {OrderId}", order.Id);
```

The value is bound through the arguments of the call, the generic type arguments of `LoggerMessage.Define`, or the
parameters of the method a `[LoggerMessage]` or `[ZLoggerMessage]` attribute decorates. A value typed as `object`,
as a generic type parameter or as an interface could hold a structured value at runtime and is not reported, and
neither is `$` on a type with its own `ToString()`, since forcing its stringification is a deliberate choice.

Does not apply to ZLogger 2.x interpolated templates: destructuring is Serilog syntax, and the `@` of a
`{value:@Name}` hole names the property instead.
