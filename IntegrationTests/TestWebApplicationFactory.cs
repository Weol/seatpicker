using System.Text;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Seatpicker.Application.Features.Login.Ports;
using Seatpicker.IntegrationTests.TestAdapters;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly ITestOutputHelper testOutputHelper;

    public TestWebApplicationFactory(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseEnvironment("Test")
            .ConfigureLogging(b =>
            {
                b.ClearProviders();
                b.SetMinimumLevel(LogLevel.Debug);
                b.AddProvider(new XUnitLoggerProvider(testOutputHelper));
            })
            .ConfigureServices(services =>
            {
                services.RemoveAll<IAuthCertificateProvider>();
                services.AddSingleton<IAuthCertificateProvider, TestAuthCertificateProvider>();


            });
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            base.Dispose(disposing);
        }
        catch (Exception)
        {
            // Ignore any exceptions caused by `Dispose`.
        }
    }
}

internal sealed class XUnitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper testOutputHelper;

    public XUnitLoggerProvider(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new XUnitLogger(testOutputHelper);
    }

    public void Dispose()
    {
    }
}

internal class XUnitLogger : ILogger
{
    private readonly LoggerExternalScopeProvider scopeProvider = new ();

    private readonly ITestOutputHelper testOutputHelper;

    public XUnitLogger(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public IDisposable BeginScope<TState>(TState state) => scopeProvider.Push(state);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var sb = new StringBuilder();
        sb
            .Append('[')
            .Append(GetLogLevelString(logLevel))
            .Append("] ")
            .Append(formatter(state, exception));

        if (exception is not null)
        {
            sb.Append("\n\n").Append(exception);
        }

        try
        {
            testOutputHelper.WriteLine(sb.ToString());
        }
        catch (InvalidOperationException e) when (e.Message == "There is no currently active test.")
        {
            // Ignore exception
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
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "FATAL",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };
    }
}
