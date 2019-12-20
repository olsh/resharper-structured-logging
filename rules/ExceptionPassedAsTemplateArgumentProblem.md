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
