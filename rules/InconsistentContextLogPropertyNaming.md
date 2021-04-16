#### Inconsistent log property naming in context (can be configured in the extension settings)

Noncompliant Code Examples:
```csharp
LogContext.PushProperty("property_name", 1);
```

Compliant Solution:
```csharp
LogContext.PushProperty("PropertyName", 1);
```
