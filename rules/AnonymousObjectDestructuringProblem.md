##### Anonymous objects must be destructured

Noncompliant Code Examples:
```csharp
Log.Error("Processed {Position}", new { x = 4, y = 2});
```

Compliant Solution:
```csharp
Log.Error("Processed {@Position}", new { x = 4, y = 2});
```
