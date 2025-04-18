# ReSharper Structured Logging
[![Build status](https://ci.appveyor.com/api/projects/status/c4riih64hbd4sebw?svg=true)](https://ci.appveyor.com/project/olsh/resharper-structured-logging)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=resharper-structured-logging&metric=alert_status)](https://sonarcloud.io/dashboard?id=resharper-structured-logging)

An extension for ReSharper and Rider IDE that highlights structured logging templates and contains some useful analyzers

> [The highlighting is a built-in feature starting from R#/Rider 2021.2](https://github.com/olsh/resharper-structured-logging/issues/35#issuecomment-900883583),
> but the extension still contains some useful analyzers that are not (yet) implemented by JetBrains team

At the moment it supports Serilog, NLog, Microsoft.Extensions.Logging and ZLogger

## Installation ReSharper

Look for `Structured Logging` in ReSharper -> Extension manager.
[JetBrains Plugins Repository](https://plugins.jetbrains.com/plugin/12083-structured-logging)

## Installation Rider

Look for `Structured Logging` in Settings -> Plugins -> Browse repositories.
[JetBrains Plugins Repository](https://plugins.jetbrains.com/plugin/12832-structured-logging)

## Highlighting

![Highlighting](https://github.com/olsh/resharper-structured-logging/raw/master/images/highlighting.png)

## Analyzers

* [Anonymous object is not destructured](rules/AnonymousObjectDestructuringProblem.md)
* [Complex object is not destructured](rules/ComplexObjectDestructuringProblem.md)
* [Complex object is not destructured in context](rules/ComplexObjectInContextDestructuringProblem.md)
* [Contextual logger mismatch](rules/ContextualLoggerProblem.md)
* [Exception passed as a template argument](rules/ExceptionPassedAsTemplateArgumentProblem.md)
* [Duplicate properties in a template](rules/TemplateDuplicatePropertyProblem.md)
* [Template should be a compile-time constant](rules/TemplateIsNotCompileTimeConstantProblem.md)
* [Prefer named properties instead of positional ones](rules/PositionalPropertyUsedProblem.md)
* [Inconsistent log property naming](rules/InconsistentLogPropertyNaming.md)
* [Inconsistent log property naming in context](rules/InconsistentContextLogPropertyNaming.md)
* [Log event messages should be fragments, not sentences](rules/LogMessageIsSentenceProblem.md)

## Turning Off Analyzers

Individual analyzers can be disabled as needed either through code comments or by adding a line to a project's
`.editorconfig` file.

### Turning Off Via Comments

The analyzer name can be used as-is in a ReSharper comment to disable an analyzer on a per-file or per-line basis.
For example:

```csharp
// ReSharper disable once TemplateIsNotCompileTimeConstantProblem
```

### Turning Off Via `.editorconfig`

To disable an analyzer for an entire directory, you can add a line to a `.editorconfig` file
([learn more](https://editorconfig.org)). In this case, the analyzer name needs to be converted to `snake_case`, prefixed
with `resharper_` and suffixed with `_highlighting`. For example:

```editorconfig
resharper_template_is_not_compile_time_constant_problem_highlighting = none
```

## Credits

Inspired by [SerilogAnalyzer](https://github.com/Suchiman/SerilogAnalyzer)
