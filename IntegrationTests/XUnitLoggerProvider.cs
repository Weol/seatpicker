using System.Text;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests;

internal sealed class XUnitLoggerProvider(ITestOutputHelper testOutputHelper) : ILoggerProvider
{
    private readonly LoggerExternalScopeProvider scopeProvider = new();

    public ILogger CreateLogger(string categoryName)
    {
        return new XUnitLogger(testOutputHelper, scopeProvider, categoryName);
    }

    public void Dispose()
    {
    }
}

internal sealed class XUnitLogger<T>(ITestOutputHelper testOutputHelper, LoggerExternalScopeProvider scopeProvider)
    : XUnitLogger(testOutputHelper, scopeProvider, typeof(T).FullName!), ILogger<T>;

internal class XUnitLogger(
    ITestOutputHelper testOutputHelper,
    LoggerExternalScopeProvider scopeProvider,
    string categoryName) : ILogger
{
    private readonly string categoryName = categoryName;

    public static ILogger CreateLogger(ITestOutputHelper testOutputHelper) =>
        new XUnitLogger(testOutputHelper, new LoggerExternalScopeProvider(), "");

    public static ILogger<T> CreateLogger<T>(ITestOutputHelper testOutputHelper) =>
        new XUnitLogger<T>(testOutputHelper, new LoggerExternalScopeProvider());

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => scopeProvider.Push(state);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception, string> formatter)
    {
        var sb = new StringBuilder();
        sb.Append(GetLogLevelString(logLevel))
            .Append(": ")
            .Append(formatter(state, exception!));

        if (exception is not null)
        {
            sb.Append('\n').Append(exception);
        }

        try
        {
            testOutputHelper.WriteLine(sb.ToString());
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "FAIL",
            LogLevel.Critical => "CRITICAL",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
        };
    }
}