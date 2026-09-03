#### Message template is not a compile time constant

Noncompliant Code Examples:
```csharp
Log.Error($"Disk quota {quota} MB exceeded by {user}");
```

```csharp
Log.Error(string.Format("Disk quota {0} MB exceeded by {1}", quota, user));
```


Compliant Solution:
```csharp
Log.Error("Disk quota {Quota} MB exceeded by {User}", quota, user);
```

Also applies to [custom logging wrappers](../README.md#custom-logging-wrappers).

Does not apply to [ZLogger](../README.md#zlogger) 2.x `ZLog*` calls: their template parameter is an
interpolated string handler, so the interpolation *is* the template and it is prepared at compile time.
An interpolated string passed to a ZLogger 1.x `format` parameter is still reported.
