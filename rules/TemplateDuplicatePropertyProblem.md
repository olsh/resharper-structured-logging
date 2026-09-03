#### Duplicate template property 

Noncompliant Code Example:
```csharp
Log.Error("Disk quota {Quota} MB exceeded by {Quota}", quota, user);
```

Compliant Solution:
```csharp
Log.Error("Disk quota {Quota} MB exceeded by {User}", quota, user);
```

Also applies to [custom logging wrappers](../README.md#custom-logging-wrappers).
