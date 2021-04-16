#### Inconsistent log property naming (can be configured in the extension settings)

Noncompliant Code Examples:
```csharp
Log.Error("Processed {property_name}", 1);
```

Compliant Solution:
```csharp
Log.Error("Processed {PropertyName}", 1);
```
