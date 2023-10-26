using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Authentication;

// ReSharper disable once InconsistentNaming
public class Login : AuthenticationTestBase
{
    public Login(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }

    [Fact]
    public async Task login_succeeds_when_discord_token_is_valid()
    {
        // Arrange
        var client = GetAnonymousClient();
        var token = "discord token";

        var discordUser = new DiscordUser("123", "Tore Tang", null);

        SetupAccessTokenResponse();
        SetupLookupResponse(discordUser);
        SetupGuildMemberResponse(discordUser);

        //Act
        var response = await client.PostAsync(
            "discord/login",
            JsonContent.Create(new DiscordAuthenticationController.LoginRequest(token)));

        var Response
            = await response.Content.ReadAsJsonAsync<DiscordAuthenticationController.TokenResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        Response.Should().NotBeNull();

        var testResponse = await TestAuthentication(client, Response!.Token);
        testResponse.Should().NotBeNull();

        Assert.Multiple(
            () => testResponse.Id.Should().Be(discordUser.Id),
            () => testResponse.Name.Should().Be(discordUser.Username),
            () => testResponse.Roles.Should().Contain(Role.User.ToString()));
    }


    [Fact]
    public async Task login_succeeds_when_discord_token_is_valid_and_has_roles_according_to_mapping()
    {
        // Arrange
        var client = GetAnonymousClient();
        var token = "discord token";

        var discordUser = new DiscordUser("123", "Tore Tang", null);

        var guildOperatorRoleId = "999999";

        var mapper = GetService<DiscordRoleMapper>();
        var options = GetService<IOptions<DiscordAuthenticationOptions>>().Value;
        await mapper.Set(options.GuildId , new[]
        {
            new DiscordRoleMapping(guildOperatorRoleId, Role.Operator),
        });

        SetupAccessTokenResponse();
        SetupLookupResponse(discordUser);
        SetupGuildMemberResponse(discordUser, guildOperatorRoleId);

        //Act
        var response = await client.PostAsync(
            "discord/login",
            JsonContent.Create(new DiscordAuthenticationController.LoginRequest(token)));
        var Response
            = await response.Content.ReadAsJsonAsync<DiscordAuthenticationController.TokenResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        Response.Should().NotBeNull();

        var testResponse = await TestAuthentication(client, Response!.Token);
        testResponse.Should().NotBeNull();

        Assert.Multiple(
            () => testResponse.Id.Should().Be(discordUser.Id),
            () => testResponse.Name.Should().Be(discordUser.Username),
            () => testResponse.Roles.Should().Contain(Role.User.ToString()),
            () => testResponse.Roles.Should().Contain(Role.Operator.ToString()));
    }
}