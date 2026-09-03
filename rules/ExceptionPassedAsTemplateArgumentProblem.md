#### Exception passed as a template argument

Noncompliant Code Example:
```csharp
catch (Exception exception)
{
   Log.Error(ex, "Disk quota {Quota} MB exceeded {Exception}", quota, exception);
}
```

Compliant Solution:
```csharp
catch (Exception exception)
{
   Log.Error(exception, "Disk quota {Quota} MB exceeded", quota);
}
```

Also applies to [custom logging wrappers](../README.md#custom-logging-wrappers).

Does not apply to [ZLogger](../README.md#zlogger) 2.x interpolated templates. The arguments after the template
are ZLogger's own `context` and the caller-info parameters the compiler fills in, so none of them is a
template argument.
