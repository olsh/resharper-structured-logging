<img src="https://github.com/olsh/resharper-structured-logging/raw/master/images/logo.png" width="64" height="64" alt="Structured Logging logo">

# ReSharper Structured Logging

[![Build](https://github.com/olsh/resharper-structured-logging/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/olsh/resharper-structured-logging/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=resharper-structured-logging&metric=alert_status)](https://sonarcloud.io/dashboard?id=resharper-structured-logging)

An extension for ReSharper and Rider IDE that highlights structured logging templates and contains some useful analyzers

At the moment it supports Serilog, NLog, Microsoft.Extensions.Logging and ZLogger,
including templates declared with `Microsoft.Extensions.Logging.LoggerMessageAttribute`,
`ZLogger.ZLoggerMessageAttribute` and `Microsoft.Extensions.Logging.LoggerMessage.Define`/`DefineScope`.

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
