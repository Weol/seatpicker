using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Seatpicker.Infrastructure;
using Seatpicker.Infrastructure.Adapters.Discord;
using Seatpicker.IntegrationTests.TestAdapters;

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
                ["Logging:LogLevel:Marten"] = "Warning",
                ["Logging:LogLevel:Npgsql"] = "Warning",
                ["Logging:LogLevel:Microsoft.AspNetCore.Routing.EndpointMiddleware"] = "Warning",

                // Values from key vault
                ["DiscordClientId"] = "9124761923842139",
                ["DiscordClientSecret"] = "<client-secret>",
                ["DiscordBotToken"] = "<bot-token>",
                ["SigningCertificate"] = GenerateSelfSignedBase64Certificate(),
                ["DatabaseAdminPassword"] = "password",
                ["DatabaseAdminUsername"] = "username",
            }))
            .ConfigureServices(services =>
            {
                // Add intercepting http message handler
                services.RemoveAll<DiscordAdapter>();
                services.AddPortMapping<DiscordAdapter, TestDiscordAdapter>();
            });
    }

    private static string GenerateSelfSignedBase64Certificate()
    {
        using var rsa = RSA.Create();
        var req = new CertificateRequest("cn=test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var certificate = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));
        return Convert.ToBase64String(certificate.Export(X509ContentType.Pfx, ""));
    }
}