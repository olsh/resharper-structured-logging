#### Exception logged as text

Logging `Message`, `StackTrace` or `ToString()` instead of the exception leaves the log event with no
exception attached, so a sink such as Seq, Sentry or Application Insights cannot group, filter or render
it as one. The type and the stack trace are lost with `Message`, and `ToString()` merely hides the whole
exception inside an unstructured string property.

Noncompliant Code Example:
```csharp
catch (Exception exception)
{
    Log.Error("Import failed {Error}", exception.Message);
    Log.Error("Import failed {Error}", exception.ToString());
    Log.Warning("Retrying after {Reason}", exception.GetBaseException().Message);
}
```

Compliant Solution:
```csharp
catch (Exception exception)
{
    Log.Error(exception, "Import failed");
    Log.Warning(exception.GetBaseException(), "Retrying");
}
```

Only those three members are reported. `exception.Data["key"]`, `exception.HResult` and a property
declared by a derived exception are scalars worth a hole of their own. Nothing is reported either once
the exception already fills the dedicated argument, as in
`Log.Error(exception, "Import failed {Error}", exception.Message)`: the exception is not lost there, and
repeating its text is a deliberate choice often enough to leave alone. When the exception object itself
fills the hole, [Exception passed as a template argument](ExceptionPassedAsTemplateArgumentProblem.md)
reports it instead.

Also applies to [custom logging wrappers](../README.md#custom-logging-wrappers), as long as the wrapper
declares an overload that takes the exception before the template.

Does not apply to [ZLogger](../README.md#zlogger) 2.x interpolated templates. The arguments after the
template are ZLogger's own `context` and the caller-info parameters the compiler fills in, so none of
them is a template argument.
