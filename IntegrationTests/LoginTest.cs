using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Seatpicker.Application.Features.Login;
using Seatpicker.Application.Features.Login.Ports;
using Xunit;

namespace Seatpicker.IntegrationTests;

public class LoginTest
{
    [Fact]
    public async Task Test1()
    {
        var host = new Host();

        //Arrange
        var discordLookupUser = host.Services.GetRequiredService<IDiscordLookupUser>();
        var discordAccessTokenProvider = host.Services.GetRequiredService<IDiscordAccessTokenProvider>();

        const string discordAccessToken = "9o3w8hj09a8jalkfjp9oa348jwopilfjap938jdosvp9a83joilnøsjmå09UJ";
        const string discordAuthorizationToken = "siu2u37ygaiusdaf";
        discordAccessTokenProvider.GetFor(discordAuthorizationToken).Returns(discordAccessToken);

        var discordUser = new DiscordUser("512831235123", "erik", "8123123123");
        discordLookupUser.Lookup(discordAccessToken).Returns(discordUser);

        //Act
        var loginService = host.Services.GetRequiredService<ILoginService>();

        var jwtToken = await loginService.GetFor(discordAuthorizationToken);
    }
}