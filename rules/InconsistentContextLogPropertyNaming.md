#### Inconsistent log property naming in context (can be configured in the extension settings)

Noncompliant Code Examples:
```csharp
// Serilog
LogContext.PushProperty("property_name", 1);

// NLog
ScopeContext.PushProperty("property_name", 1);
logger.PushScopeProperty("property_name", 1);
```

Compliant Solution:
```csharp
// Serilog
LogContext.PushProperty("PropertyName", 1);

// NLog
ScopeContext.PushProperty("PropertyName", 1);
logger.PushScopeProperty("PropertyName", 1);
```
