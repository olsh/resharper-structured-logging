#### Incorrect type is used for contextual logger

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
    private static readonly ILogger Logger = Log.ForContext<B>();
}

class B { }
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

```csharp
class A(ILogger<B> log)
{
	ILogger<B> _log = log;
}

class B { } 
```

```csharp
class A
{
	ILogger _log;

	public A(ILoggerFactory loggerFactory)
	{
		_log = loggerFactory.CreateLogger<B>();
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
    private static readonly ILogger Logger = Log.ForContext<A>();
}

class B { }
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

```csharp
class A(ILogger<A> log)
{
	ILogger<A> _log = log;
}

class B {} 
```

```csharp
class A
{
	ILogger _log;

	public A(ILoggerFactory loggerFactory)
	{
		_log = loggerFactory.CreateLogger<A>();
	}
}

class B { }
```
