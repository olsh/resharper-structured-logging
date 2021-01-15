#### Inconsistent log property naming (can be configured in the extension settings, at the moment the options page is available only in the R# version https://github.com/olsh/resharper-structured-logging/issues/18)

Noncompliant Code Examples:
```csharp
Log.Error("Processed {property_name}", 1);
```

Compliant Solution:
```csharp
Log.Error("Processed {PropertyName}", 1);
```
