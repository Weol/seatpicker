using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Azure;
using Azure.Data.Tables;
using IntegrationTests.Host;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Seatpicker.Adapters.Adapters;
using Seatpicker.Domain.Domain.Registration;
using Xunit;

namespace IntegrationTests;

public class LoginTest
{
    [Fact]
    public async Task Test1()
    {
        var host = new Host.Host();
        host.Services.GetRequiredService<DiscordClientFaker>().SetupDiscordUser(
            "asd", 
            "asd", 
            null);

        var loginService = host.Services.GetRequiredService<ILoginService>();

        var jwt = await loginService.GetFor("testDiscordToken");
    }

    private record LoginRequestModel(string Token);

    private record LoginResponseModel(string Token);
}