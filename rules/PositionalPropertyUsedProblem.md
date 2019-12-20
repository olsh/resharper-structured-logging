#### Prefer named properties instead of positional ones

Noncompliant Code Examples:
```csharp
Log.Error("Disk quota {0} MB exceeded by {1}", quota, user);
```


Compliant Solution:
```csharp
Log.Error("Disk quota {Quota} MB exceeded by {User}", quota, user);
```
