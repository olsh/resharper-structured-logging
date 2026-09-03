#### Complex objects with default `ToString()` implementation probably need to be destructured 

Noncompliant Code Example:
```csharp
class User
{
    public int Age { get; set; }
}

...

Log.Information("The user is {MyUser}", new User());
```

Compliant Solution:
```csharp
class User
{
    public int Age { get; set; }
}

...

Log.Information("The user is {@MyUser}", new User());

// or

Log.Information("The user is {$MyUser}", new User());
```

Also applies to [custom logging wrappers](../README.md#custom-logging-wrappers).

Does not apply to [ZLogger](../README.md#zlogger) 2.x interpolated templates: destructuring is Serilog syntax,
ZLogger serializes a value with the `:json` format specifier instead.
