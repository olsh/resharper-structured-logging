<img src="https://github.com/olsh/resharper-structured-logging/raw/master/images/logo.png" width="64" height="64" alt="Structured Logging logo">

# ReSharper Structured Logging

[![Build](https://github.com/olsh/resharper-structured-logging/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/olsh/resharper-structured-logging/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=resharper-structured-logging&metric=alert_status)](https://sonarcloud.io/dashboard?id=resharper-structured-logging)

An extension for ReSharper and Rider IDE that highlights structured logging templates and contains some useful analyzers

At the moment it supports Serilog, NLog, Microsoft.Extensions.Logging and [ZLogger](#zlogger),
including templates declared with `Microsoft.Extensions.Logging.LoggerMessageAttribute`,
`ZLogger.ZLoggerMessageAttribute` and `Microsoft.Extensions.Logging.LoggerMessage.Define`/`DefineScope`.
[Custom logging wrappers](#custom-logging-wrappers) are supported as well

## Analyzers

| Analyzer | Quick fix | Adopted by R#/Rider |
| --- | :---: | --- |
| [Message template highlighting](#highlighting) | — | [2021.2](https://www.jetbrains.com/help/resharper/Code_Analysis__String_Formatting_Methods.html) |
| [Anonymous object is not destructured](rules/AnonymousObjectDestructuringProblem.md) | ✔ | — |
| [Complex object is not destructured](rules/ComplexObjectDestructuringProblem.md) | ✔ | — |
| [Complex object is not destructured in context](rules/ComplexObjectInContextDestructuringProblem.md) | ✔ | — |
| [Contextual logger mismatch](rules/ContextualLoggerProblem.md) | ✔ | — |
| [Exception passed as a template argument](rules/ExceptionPassedAsTemplateArgumentProblem.md) | ✔ | — |
| [Duplicate properties in a template](rules/TemplateDuplicatePropertyProblem.md) | ✔ | [2025.2](https://www.jetbrains.com/help/resharper/DuplicateItemInLoggerTemplate.html), Serilog-style calls only |
| [Template should be a compile-time constant](rules/TemplateIsNotCompileTimeConstantProblem.md) | ✔ | [2025.1](https://www.jetbrains.com/help/resharper/NonStaticLoggerTemplate.html), as a hint |
| [Prefer named properties instead of positional ones](rules/PositionalPropertyUsedProblem.md) | ✔ | — |
| [Inconsistent log property naming](rules/InconsistentLogPropertyNaming.md) | ✔ | — |
| [Inconsistent log property naming in context](rules/InconsistentContextLogPropertyNaming.md) | ✔ | — |
| [Log event messages should be fragments, not sentences](rules/LogMessageIsSentenceProblem.md) | ✔ | — |

The last column names the ReSharper/Rider release that adopted the feature. Where a version is listed
the extension either no longer provides the feature at all, or still reports it because the built-in
inspection does not fully replace it:

* Duplicate properties are reported by the IDE for calls such as `Log.Information("{Id} {Id}", ...)`,
  but not for templates declared with `LoggerMessageAttribute`.
* A template that is not a compile-time constant is reported by the IDE as a hint tied to
  [CA2254](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2254),
  while the extension reports it as a warning. The quick fix comes from ReSharper.

## Custom Logging Wrappers

Wrapping a logger in a helper of your own normally hides the message template from the analyzers, because the helper is
not one of the methods listed above. Annotating the helper brings the highlighting and every analyzer back:

```csharp
public static class LoggerExtensions
{
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void LogInformation(this ILogger logger, string messageTemplate, params object[] propertyValues)
    {
        logger.Information(messageTemplate, propertyValues);
    }
}
```

`MessageTemplateFormatMethodAttribute` ships with Serilog (`Serilog.Core`) and with NLog (`NLog`). Only the attribute
name is matched, so a project that references neither can declare its own in any namespace:

```csharp
[AttributeUsage(AttributeTargets.Method)]
public sealed class MessageTemplateFormatMethodAttribute : Attribute
{
    public MessageTemplateFormatMethodAttribute(string messageTemplateParameterName)
    {
        MessageTemplateParameterName = messageTemplateParameterName;
    }

    public string MessageTemplateParameterName { get; }
}
```

Alternatively, mark the template parameter with `StructuredMessageTemplateAttribute` from
[JetBrains.Annotations](https://www.nuget.org/packages/JetBrains.Annotations), the annotation the built-in R#/Rider
template highlighting also understands:

```csharp
public static void LogInformation(
    this ILogger logger,
    [StructuredMessageTemplate] string messageTemplate,
    params object[] propertyValues)
```

An exception parameter declared before the template parameter is recognized on a wrapper too, so
[exception passed as a template argument](rules/ExceptionPassedAsTemplateArgumentProblem.md) keeps working on the
wrapper's overloads

## ZLogger

ZLogger 1.x takes the template as a string, so every analyzer applies to it as it does to any other logger.

ZLogger 2.x replaced those overloads with interpolated string handlers, and its templates are interpolated
strings instead: `logger.ZLogInformation($"Connected to {host}")`. The property name of a hole is the source
text of its expression, unless the format specifier gives one explicitly:

```csharp
// logs the property `host`
logger.ZLogInformation($"Connected to {host}");

// logs the property `Host`
logger.ZLogInformation($"Connected to {host:@Host}");

// logs the property `StartedAt`, rendered with the `yyyy-MM-dd` format
logger.ZLogInformation($"Started at {startedAt:@StartedAt:yyyy-MM-dd}");
```

[Inconsistent log property naming](rules/InconsistentLogPropertyNaming.md),
[duplicate properties](rules/TemplateDuplicatePropertyProblem.md),
[log event messages should be fragments](rules/LogMessageIsSentenceProblem.md) and
[statement dimming](#dimming-logging-statements) apply to these call sites. The remaining analyzers do not:
an interpolated template is a compile-time constant by construction, it has no positional properties,
ZLogger serializes with `:json` rather than with Serilog's destructuring operators, and the arguments that
follow the template are ZLogger's own `context` and caller-info parameters rather than template arguments.

The naming analyzer only reports a hole it could suggest a rename for: one with an explicit `:@name`, or one
holding a plain identifier. `$"{user.Name}"` and `$"{GetCount()}"` are logged under those expressions
verbatim, but they are left alone, because no property name could be suggested for them. They still count as
[duplicate properties](rules/TemplateDuplicatePropertyProblem.md) when the same expression is repeated.

Quick fixes are not offered on an interpolated template, only the warnings.

## Dimming Logging Statements

Logging statements can be greyed out, the way unreachable code is rendered, so that they stand out less than the
surrounding code. The option is off by default; enable `Dim logging statements` in
Settings -> Environment -> Structured Logging.

Only a statement that consists of nothing but a logging call is dimmed, so a logging call feeding a larger expression
keeps its usual colors. Analysis squiggles stay visible on dimmed statements.

## Highlighting

Adopted by ReSharper and Rider in 2021.2, so message templates are highlighted out of the box and the
extension no longer provides it.

![Highlighting](https://github.com/olsh/resharper-structured-logging/raw/master/images/highlighting.png)

## Installation ReSharper

Look for `Structured Logging` in ReSharper -> Extension manager.
[JetBrains Plugins Repository](https://plugins.jetbrains.com/plugin/12083-structured-logging)

## Installation Rider

Look for `Structured Logging` in Settings -> Plugins -> Browse repositories.
[JetBrains Plugins Repository](https://plugins.jetbrains.com/plugin/12832-structured-logging)

## Turning Off Analyzers

Individual analyzers can be disabled as needed either through code comments or by adding a line to a project's
`.editorconfig` file.

### Turning Off Via Comments

The analyzer name can be used as-is in a ReSharper comment to disable an analyzer on a per-file or per-line basis. For
example:

```csharp
// ReSharper disable once TemplateIsNotCompileTimeConstantProblem
```

### Turning Off Via `.editorconfig`

To disable an analyzer for an entire directory, you can add a line to a `.editorconfig` file
([learn more](https://editorconfig.org)). In this case, the analyzer name needs to be converted to `snake_case`,
prefixed with `resharper_` and suffixed with `_highlighting`. For example:

```editorconfig
resharper_template_is_not_compile_time_constant_problem_highlighting = none
```

## Credits

Inspired by [SerilogAnalyzer](https://github.com/Suchiman/SerilogAnalyzer)
