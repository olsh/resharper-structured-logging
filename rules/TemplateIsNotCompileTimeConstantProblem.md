##### Message template is not a compile time constant

Noncompliant Code Examples:
```csharp
Log.Error($"Disk quota {quota} MB exceeded by {user}");
```

```csharp
Log.Error(string.Format("Disk quota {0} MB exceeded by {1}", quota, user));
```


Compliant Solution:
```csharp
Log.Error("Disk quota {Quota} MB exceeded by {User}", quota, user);
```
