#### Inconsistent log property naming (can be configured in the extension settings)

Noncompliant Code Examples:
```csharp
Log.Error("Processed {property_name}", 1);
```

Compliant Solution:
```csharp
Log.Error("Processed {PropertyName}", 1);
```

Also applies to [custom logging wrappers](../README.md#custom-logging-wrappers).

On a [ZLogger](../README.md#zlogger) 2.x call the hole is named after its expression, so the name is set with
the `:@` format specifier:

```csharp
logger.ZLogError($"Processed {propertyName}");            // logs `propertyName`
logger.ZLogError($"Processed {propertyName:@PropertyName}");
```

Only a hole that carries an explicit `:@name` or holds a plain identifier is reported, and no quick fix is
offered for one.
