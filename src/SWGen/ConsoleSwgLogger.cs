﻿using Microsoft.Extensions.Logging;

namespace SWGen;

public class ConsoleSwgLogger : ISwgLogger
{
    private readonly bool _enableDebug;
    private readonly string[] _scopes;

    public ConsoleSwgLogger(bool enableDebug, params string[] scopes)
    {
        _enableDebug = enableDebug;
        _scopes = scopes;
    }

    public void Log(LogLevel logLevel, string message)
    {
        if (logLevel == LogLevel.Debug && !_enableDebug) return;
        var color = logLevel switch
        {
            LogLevel.Information => ConsoleColor.Green,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Debug => ConsoleColor.DarkGray,
            _ => ConsoleColor.White
        };
        var threeLetterLevelName = logLevel switch
        {
            LogLevel.Debug => "DBG",
            LogLevel.Information => "INF",
            LogLevel.Error => "ERR",
            _ => "???"
        };

        ColorLog(color, DateTime.Now, threeLetterLevelName, message);
    }

    private void ColorLog(ConsoleColor color, DateTime eventTime, string level, string message) =>
        ColorWriteLine(
            color,
            _scopes.Length > 0
                ? $"[{eventTime:HH:mm:ss} {_scopes.StringJoin("|")}] {message}"
                : $"[{eventTime:HH:mm:ss}] {message}");

    private static void ColorWriteLine(ConsoleColor color, string message)
    {
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = oldColor;
    }

    ISwgLogger ISwgLogger.BeginScope(string scopeName) => new ConsoleSwgLogger(_enableDebug, [.._scopes, scopeName]);
}