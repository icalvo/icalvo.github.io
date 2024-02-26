namespace SWGen;

public static class Logger
{
    public static void Info(string message) => Info(DateTime.Now, message);

    public static void Error(string message) => Error(DateTime.Now, message);

    public static void Info(DateTime eventTime, string message) => ColorLog(ConsoleColor.Green, eventTime, "INF", message);

    public static void Error(DateTime eventTime, string message) => ColorLog(ConsoleColor.Red, eventTime, "ERR", message);

    public static void Debug(string message) => ColorLog(ConsoleColor.DarkGray, DateTime.Now, "DBG", message);

    private static void ColorLog(ConsoleColor color, DateTime eventTime, string level, string message) =>
        ColorWriteLine(color, $"[{eventTime:HH:mm:ss} {Scopes.Reverse().StringJoin("/")}] {message}");

    private static void ColorWriteLine(ConsoleColor color, string message)
    {
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = oldColor;
    }

    private static readonly Stack<string> Scopes = new();
    public static IDisposable BeginScope(string scopeName)
    {
        Scopes.Push(scopeName);
        return new ActionDisposable(() => _ = Scopes.Pop());
    }
    
    private class ActionDisposable : IDisposable
    {
        private readonly Action _action;
        public ActionDisposable(Action action) => _action = action;
        public void Dispose() => _action();
    }
}