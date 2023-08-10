﻿using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Seatpicker.Application.Features;
using Seatpicker.Infrastructure;
using Seatpicker.Infrastructure.Adapters.DiscordClient;
using Seatpicker.IntegrationTests.TestAdapters;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Infrastructure.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseEnvironment("Test")
            .ConfigureAppConfiguration(b => b.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Values from key vault
                ["DiscordClientId"] = "9124761923842139",
                ["DiscordClientSecret"] = "<client-secret>",
                ["SigningCertificate"] = GenerateSelfSignedBase64Certificate(),

                ["Discord:Uri"] = "https://discord.com/api/v10",
                ["Discord:RedirectUri"] = "http://localhost.discord.redirect",

                ["Database:ConnectionString"] = "Server=postgres.local;Database=;Port=5432;Database=postgres;Username=username;Password=password;Ssl Mode=Allow;",

                ["Logging:LogLevel:Default"] = "Error",
                ["Logging:LogLevel:Seatpicker"] = "Trace",
            }))
            .ConfigureServices(services =>
            {
                // Replace aggregate repository
                services.RemoveAll<IAggregateRepository>();
                services.AddPortMapping<IAggregateRepository, TestAggregateRepository>();

                // Add identity generator
                services.AddSingleton<IdentityGenerator>();

                // Add intercepting http message handler
                var interceptingHandler = Substitute.ForPartsOf<InterceptingHttpMessageHandler>();
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