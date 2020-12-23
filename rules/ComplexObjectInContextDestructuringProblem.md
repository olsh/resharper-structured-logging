#### Complex objects with default `ToString()` implementation probably need to be destructured

Noncompliant Code Example:
```csharp
class User
{
    public int Age { get; set; }
}

...

LogContext.PushProperty("User", new User());
```

Compliant Solution:
```csharp
class User
{
    public int Age { get; set; }
}

...

LogContext.PushProperty("User", new User(), true);

// or

LogContext.PushProperty("User", new User(), false);
```
