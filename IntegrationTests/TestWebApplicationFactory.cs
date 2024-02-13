using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using JasperFx.Core;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using NSubstitute;
using Seatpicker.Infrastructure.Adapters.Database;
using Seatpicker.IntegrationTests.HttpInterceptor;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssemblyFixture;

namespace Seatpicker.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Infrastructure.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseEnvironment("Development")
            .ConfigureAppConfiguration(b => b.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:LogLevel:Default"] = "Information",
                ["Logging:LogLevel:Seatpicker"] = "Debug",
                ["Logging:LogLevel:Marten"] = "Information",
                
                // Values from key vault
                ["DiscordClientId"] = "9124761923842139",
                ["DiscordClientSecret"] = "<client-secret>",
                ["DiscordBotToken"] = "<bot-token>",
                ["SigningCertificate"] = GenerateSelfSignedBase64Certificate(),
                ["DatabaseAdminPassword"] = "password",
                ["DatabaseAdminUsername"] = "username",
                
                ["Logging:LogLevels:Default"] = "Information"
            }))
            .ConfigureServices(services =>
            {
                // Add intercepting http message handler
                var interceptingHandler = new InterceptingHttpMessageHandler();
                services.AddSingleton(interceptingHandler);
                services.ConfigureAll<HttpClientFactoryOptions>(
                    options => options.HttpMessageHandlerBuilderActions.Add(
                        handlerBuilder => handlerBuilder.PrimaryHandler = interceptingHandler));
            });
    }

    private static string GenerateSelfSignedBase64Certificate()
    {
        var rsa = RSA.Create();
        var req = new CertificateRequest("cn=test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var certificate = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));
        return Convert.ToBase64String(certificate.Export(X509ContentType.Pfx, ""));
    }
}