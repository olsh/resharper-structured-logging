# ReSharper Structured Logging
[![Build status](https://ci.appveyor.com/api/projects/status/c4riih64hbd4sebw?svg=true)](https://ci.appveyor.com/project/olsh/resharper-structured-logging)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=resharper-structured-logging&metric=alert_status)](https://sonarcloud.io/dashboard?id=resharper-structured-logging)

An extension for ReSharper and Rider IDE that highlights structured logging templates and contains some useful analyzers

At the moment it supports Serilog, NLog, and Microsoft.Extensions.Logging

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
* [Log event messages should be fragments, not sentences](rules/InconsistentLogPropertyNaming.md) 

## Credits

Inspired by [SerilogAnalyzer](https://github.com/Suchiman/SerilogAnalyzer)
