using Microsoft.Extensions.DependencyInjection;
using Seatpicker.IntegrationTests.Host.Adapters;
using Seatpicker.UserContext.Domain.Registration;
using Xunit;

namespace Seatpicker.IntegrationTests;

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