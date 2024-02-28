using Microsoft.Extensions.Logging;

namespace SWGen;

public interface ISwgLogger
{
    void Log(LogLevel logLevel, string message);
    public void Info(string message) => Log(LogLevel.Information, message);

    public void Error(string message) => Log(LogLevel.Error, message);
    public void Debug(string message) => Log(LogLevel.Debug, message);

    public ISwgLogger BeginScope(string scopeName);
}