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
