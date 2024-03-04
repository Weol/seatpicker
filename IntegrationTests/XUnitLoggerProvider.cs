using System.Text;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests;

internal sealed class XUnitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper testOutputHelper;
    private readonly LoggerExternalScopeProvider scopeProvider = new();

    public XUnitLoggerProvider(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new XUnitLogger(testOutputHelper, scopeProvider, categoryName);
    }

    public void Dispose()
    {
    }
}

internal sealed class XUnitLogger<T> : XUnitLogger, ILogger<T>
{
    public XUnitLogger(ITestOutputHelper testOutputHelper, LoggerExternalScopeProvider scopeProvider)
        : base(testOutputHelper, scopeProvider, typeof(T).FullName!)
    {
    }
}

internal class XUnitLogger : ILogger
{
    private readonly ITestOutputHelper testOutputHelper;
    private readonly string categoryName;
    private readonly LoggerExternalScopeProvider scopeProvider;

    public static ILogger CreateLogger(ITestOutputHelper testOutputHelper) =>
        new XUnitLogger(testOutputHelper, new LoggerExternalScopeProvider(), "");

    public static ILogger<T> CreateLogger<T>(ITestOutputHelper testOutputHelper) =>
        new XUnitLogger<T>(testOutputHelper, new LoggerExternalScopeProvider());

    public XUnitLogger(
        ITestOutputHelper testOutputHelper,
        LoggerExternalScopeProvider scopeProvider,
        string categoryName)
    {
        this.testOutputHelper = testOutputHelper;
        this.scopeProvider = scopeProvider;
        this.categoryName = categoryName;
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public IDisposable BeginScope<TState>(TState state) => scopeProvider.Push(state);

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