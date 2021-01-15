#### Inconsistent log property naming in context (can be configured in the extension settings, at the moment the options page is available only in the R# version https://github.com/olsh/resharper-structured-logging/issues/18)

Noncompliant Code Examples:
```csharp
LogContext.PushProperty("property_name", 1);
```

Compliant Solution:
```csharp
LogContext.PushProperty("PropertyName", 1);
```
