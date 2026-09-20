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
| [Destructuring or stringification operator has no effect on a scalar value](rules/RedundantDestructuringOperatorProblem.md) | ✔ | — |
| [Contextual logger mismatch](rules/ContextualLoggerProblem.md) | ✔ | — |
| [Exception passed as a template argument](rules/ExceptionPassedAsTemplateArgumentProblem.md) | ✔ | — |
| [Exception logged as text](rules/ExceptionLoggedAsTextProblem.md) | ✔ | — |
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

## Code Completion

A property gets its name the moment the hole is typed, so that is where the name is offered. With the caret
inside `{` of a message template, code completion lists names built from the arguments that fill the holes:

```csharp
_logger.LogInformation("Shipped {", order.Id, customer.Email);
//                              ^ OrderId, Id, CustomerEmail, Email
```

The names for the argument this very hole will be bound to come first, and each argument offers both its
qualified name and its leaf one, so `order.Customer.Name` suggests `CustomerName` and then `Name`. They are
already in the case configured under Settings -> Environment -> Structured Logging, the same one
[the naming analyzer](rules/InconsistentLogPropertyNaming.md) holds the template to. A name another hole of
the template already uses is left out, and accepting one closes the hole when the brace is missing, leaving
the caret past it. The list appears after a destructuring or stringification operator as well, at `{@` and
`{$`, and when completing the name of a hole that is already closed.

Nothing is offered where no name can be derived: for a positional hole such as `{0}`, which
[renaming it](rules/PositionalPropertyUsedProblem.md) is the answer to, for a template whose hole values are
passed as one array instead of being expanded, for a concatenated template, and once every argument is
already bound to a hole.

Templates declared with `LoggerMessageAttribute` are completed by ReSharper and Rider themselves, from the
parameters of the method the attribute decorates, which is the right source there because no argument fills
those holes. `LoggerMessage.Define` binds its holes to generic type arguments and ZLogger 2.x binds them to
the interpolations of the template, so neither has an expression to name a hole after.

## Custom Logging Wrappers

A project that logs through its own wrapper rather than calling the logger directly is analyzed as well, once
the wrapper says which parameter carries the template. Annotate the method with
`MessageTemplateFormatMethodAttribute`, naming the parameter:

```csharp
[MessageTemplateFormatMethod("messageTemplate")]
public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
```

or annotate the parameter itself with JetBrains.Annotations' `StructuredMessageTemplateAttribute`, which the
built-in ReSharper and Rider template highlighting understands too:

```csharp
public void LogError(Exception exception, [StructuredMessageTemplate] string messageTemplate, params object[] propertyValues)
```

Either way the arguments that follow the template parameter are taken as the hole values, so a wrapper is held
to the same rules as a direct call. Rules that move an argument, such as
[Exception passed as a template argument](rules/ExceptionPassedAsTemplateArgumentProblem.md), additionally need
the wrapper to declare an overload that takes the exception before the template.

## ZLogger

ZLogger 1.x `ZLog*` calls take a plain `string format` and behave like any other template.

ZLogger 2.x replaced those overloads with an interpolated string handler, so the template and its holes live
inside the interpolated string:

```csharp
logger.ZLogError($"Could not open socket {host:@Host} on {port}");
```

The holes are the interpolations, named after `:@name` where one is given and after the source text of the
expression otherwise, and the parameters that follow the template are ZLogger's own `context` together with the
caller-info arguments the compiler fills in. That changes what several rules mean, and each rule documents how
it applies. Destructuring is Serilog syntax and has no counterpart here: ZLogger serializes with the `:json`
format instead, and the `@` of a hole introduces a name.

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
