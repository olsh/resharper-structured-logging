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
