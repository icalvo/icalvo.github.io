---
layout: post
title: "Logging with Microsoft"
date: 2019-03-07
comments: true
categories: [software]
tags: [c#,dotnet,logging]
---
# Introduction
In recent times, container deployment has become a standard process, usually with some kind of container orchestration technology like Kubernetes. This allows a smart logging strategy which is to capture the console output of the container and then using a different common container to redirect it to whichever storage or logging platform we want.

Therefore, suddenly the features of advanced logging platforms like NLog or Serilog have lost a bit of importance, since we don't need nothing but console logging.

Microsoft .NET comes with a default logging platform that includes console logging and I think this is more than enough for a majority of cases. Let's take a look at what features it has and how to configure it.

# Basic abstractions

## `ILogger`
The main abstraction to consider is `ILogger`. This class will be used to log messages and comes with three methods:

```csharp
IDisposable BeginScope<TState>(TState state)
```
You can use this to start a scope. You can also nest scopes. The implementation can then create a `Breadcrumb` of scopes, from the outermost one to the innermost, and make it available when the logging commands come.

The scope finishes when you dispose it, and it is expected to dispose scopes in the same order you began them, so it is very advisable to use the `using` statement to ensure that you finish your scopes properly and in order.

```csharp
void Log<TState>(
      LogLevel logLevel,
      EventId eventId,
      TState state,
      Exception? exception,
      Func<TState, Exception?, string> formatter);
```
This is the method to log a message. You have several simplified versions in the form of extension methods, but all of them are implemented against this method.

As you can see, the thing that you log is not necessarily a string but a generic `state` object. That's why you are asked to provide a formatting function that gets a string out of that state and the optional `Exception` that caused it.

```csharp
bool IsEnabled(LogLevel logLevel);
```

This informs whether the logger is enabled for a given log level. This allows the client to avoid some complex logic to compose a log message (or a state, in general), if the desired log level is not enabled in the logger.

## `ILoggerProvider`

The implementors of this interface are meant to provide and keep track of the instances of `ILogger`. The interface has a single method:

```csharp
ILogger CreateLogger(string categoryName);
```

`ILogger` instances have an category name. It is very usual that the category corresponds with a class.

Usually, implementors create a single logger instance per category, with the help of a `ConcurrentDictionary`. It is also at this level that the scopes are tracked. In this way all the loggers provided will have the same scope breadcrumb available.

# Console logging
The starting point of console logging is `ConsoleLoggerProvider`. This provider returns instances of `ConsoleLogger`. This logger uses an implementation of the abstract class `ConsoleFormatter` to actually format and print the messages. All `ConsoleFormatter` has one abstract method:

```csharp
public abstract void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter);
```

## `logEntry`
`logEntry` is an object that basically holds all the parameters that are received at `ConsoleLogger.Log()`, plus the logger category.

```csharp
public LogLevel LogLevel { get; }
public string Category { get; }
public EventId EventId { get; }
public TState State { get; }
public Exception? Exception { get; }
public Func<TState, Exception?, string>? Formatter { get; }
```

## `scopeProvider`
`scopeProvider` is the scope manager. The methods of `IExternalScopeProvider` are:

```csharp
void ForEachScope<TState>(Action<object?, TState> callback, TState state);
IDisposable Push(object? state);
```
Calls to `BeginScope()` are forwarded to `Push()`. `ForEachScope()` allows you to iterate through the current scope breadcrumb. The `state` parameter allows you to provide an object that will be handed to each piece of the breadcrumb. This is useful for avoiding lambda closures, e.g:

```csharp
var scopes = new List<string>();
scopeProvider.ForEachScope((item, _) => scopes.Add(item.ToString()), (string)null);
```
Here you don't make use of the `state` parameter, but on the other hand, `scopes` becomes a closure of the lambda expression, which, as you may know, it is something you usually want to avoid, or at least be very careful with.

The solution therefore is:
```csharp
var scopes = new List<string>();
scopeProvider.ForEachScope((item, s) => s.Add(item.ToString()), scopes);
```

## `textWriter`
Finally, the `textWriter` parameter is where you are going to write your output. This means that if you do your own implementation of `ConsoleFormatter`, you cannot use `Console.ForegroundColor` and `Console.BackgroundColor`. Instead, you will need to use ANSI codes directly. The following class comes handy for that task:

```csharp
    internal static class TextWriterExtensions
    {
        private const string DefaultForegroundColor = "\u001B[39m\u001B[22m";
        private const string DefaultBackgroundColor = "\u001B[49m";

        public static void WriteColoredMessage(this TextWriter textWriter, string message, ConsoleColor? background, ConsoleColor? foreground)
        {
            if (background.HasValue)
            {
                textWriter.Write(background.Value.GetBackgroundColorEscapeCode());
            }

            if (foreground.HasValue)
            {
                textWriter.Write(foreground.Value.GetForegroundColorEscapeCode());
            }

            textWriter.Write(message);
            if (foreground.HasValue)
            {
                textWriter.Write(DefaultForegroundColor);
            }

            if (background.HasValue)
            {
                textWriter.Write(DefaultBackgroundColor);
            }
        }

        private static string GetForegroundColorEscapeCode(this ConsoleColor color) =>
            color switch
            {
                ConsoleColor.Black => "\u001B[30m",
                ConsoleColor.DarkBlue => "\u001B[34m",
                ConsoleColor.DarkGreen => "\u001B[32m",
                ConsoleColor.DarkCyan => "\u001B[36m",
                ConsoleColor.DarkRed => "\u001B[31m",
                ConsoleColor.DarkMagenta => "\u001B[35m",
                ConsoleColor.DarkYellow => "\u001B[33m",
                ConsoleColor.Gray => "\u001B[37m",
                ConsoleColor.Blue => "\u001B[1m\u001B[34m",
                ConsoleColor.Green => "\u001B[1m\u001B[32m",
                ConsoleColor.Cyan => "\u001B[1m\u001B[36m",
                ConsoleColor.Red => "\u001B[1m\u001B[31m",
                ConsoleColor.Magenta => "\u001B[1m\u001B[35m",
                ConsoleColor.Yellow => "\u001B[1m\u001B[33m",
                ConsoleColor.White => "\u001B[1m\u001B[37m",
                _ => "\u001B[39m\u001B[22m"
            };

        private static string GetBackgroundColorEscapeCode(this ConsoleColor color) =>
            color switch
            {
                ConsoleColor.Black => "\u001B[40m",
                ConsoleColor.DarkBlue => "\u001B[44m",
                ConsoleColor.DarkGreen => "\u001B[42m",
                ConsoleColor.DarkCyan => "\u001B[46m",
                ConsoleColor.DarkRed => "\u001B[41m",
                ConsoleColor.DarkMagenta => "\u001B[45m",
                ConsoleColor.DarkYellow => "\u001B[43m",
                ConsoleColor.Gray => "\u001B[47m",
                _ => "\u001B[49m"
            };
    }
```

# An example of `ConsoleFormatter`

