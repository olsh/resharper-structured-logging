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

On a [ZLogger](../README.md#zlogger) 2.x call two holes collide when they resolve to the same name, whether
the name comes from a `:@` specifier or from the expression itself:

```csharp
logger.ZLogError($"Disk quota {quota:@Quota} MB exceeded by {user:@Quota}");
logger.ZLogError($"Disk quota {limit.Quota} MB exceeded by {limit.Quota}");
```
