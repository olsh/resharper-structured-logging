#### Anonymous objects must be destructured

Noncompliant Code Examples:
```csharp
Log.Error("Processed {Position}", new { x = 4, y = 2});
```

Compliant Solution:
```csharp
Log.Error("Processed {@Position}", new { x = 4, y = 2});
```

Also applies to [custom logging wrappers](../README.md#custom-logging-wrappers).

Does not apply to [ZLogger](../README.md#zlogger) 2.x interpolated templates: destructuring is Serilog syntax,
ZLogger serializes a value with the `:json` format specifier instead.
