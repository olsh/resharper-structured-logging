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

### Duplicate template property 

Noncompliant Code Example:
```csharp
Log.Error("Disk quota {Quota} MB exceeded by {Quota}", quota, user);
```

Compliant Solution:
```csharp
Log.Error("Disk quota {Quota} MB exceeded by {User}", quota, user);
```

### Exception passed as a template argument

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

### Message template is not a compile time constant

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

### Anonymous objects must be destructured

Noncompliant Code Examples:
```csharp
Log.Error("Processed {Position}", new { x = 4, y = 2});
```

Compliant Solution:
```csharp
Log.Error("Processed {@Position}", new { x = 4, y = 2});
```

### Incorrect type is used for contextual logger

Noncompliant Code Examples:
```csharp
class A
{
    private static readonly ILogger Logger = Logger.ForContext<B>();
}

class B {} 
```

```csharp
class A
{
	ILogger<B> _log;
	
	public A(ILogger<B> log)
	{
		_log = log;
	}
}

class B { } 
```

Compliant Solution:
```csharp
class A
{
    private static readonly ILogger Logger = Logger.ForContext<A>();
}

class B {} 
```

```csharp
class A
{
	ILogger<A> _log;
	
	public A(ILogger<A> log)
	{
		_log = log;
	}
}

class B {} 
```

## Credits

Inpired by [SerilogAnalyzer](https://github.com/Suchiman/SerilogAnalyzer)
