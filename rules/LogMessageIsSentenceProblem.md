#### Log event messages should be fragments, not sentences

[https://benfoster.io/blog/serilog-best-practices/#message-template-recommendations](https://benfoster.io/blog/serilog-best-practices/#message-template-recommendations)

Noncompliant Code Examples:
```csharp
Log.Error("Disk quota {Quota} MB exceeded by {User}.", quota, user);
```


Compliant Solution:
```csharp
Log.Error("Disk quota {Quota} MB exceeded by {User}", quota, user);
```

Also applies to [custom logging wrappers](../README.md#custom-logging-wrappers).

Also applies to [ZLogger](../README.md#zlogger) 2.x interpolated templates, where no quick fix is offered:

```csharp
logger.ZLogError($"Disk quota {quota:@Quota} MB exceeded by {user:@User}.");
```
