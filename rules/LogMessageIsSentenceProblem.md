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
