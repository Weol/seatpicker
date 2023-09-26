using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Authentication;

// ReSharper disable once InconsistentNaming
public class Renew : AuthenticationTestBase
{
    public Renew(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }

    [Fact]
    public async Task renew_succeeds_when_refresh_token_is_valid()
    {
        // Arrange
        var client = GetAnonymousClient();
        var refreshToken = "discord token";

        var discordUser = new DiscordUser("123", "Tore Tang", null);

        SetupRefreshResponse();
        SetupLookupResponse(discordUser);
        SetupGuildMemberResponse(discordUser);

        //Act
        var response = await client.PostAsync(
            "discord/renew",
            JsonContent.Create(new DiscordAuthenticationController.RenewRequest(refreshToken)));
        var Response = await response.Content.ReadAsJsonAsync<DiscordAuthenticationController.TokenResponse>();

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
    public async Task renew_succeeds_when_refresh_token_is_valid_and_has_roles_according_to_mapping()
    {
        // Arrange
        var client = GetAnonymousClient();
        var refreshToken = "discord token";

        var discordUser = new DiscordUser("123", "Tore Tang", null);

        var guildOperatorRoleId = "9999999";

        var mapper = GetService<DiscordRoleMapper>();
        var options = GetService<IOptions<DiscordAuthenticationOptions>>().Value;
        await mapper.Set(options.GuildId, new[]
        {
            new DiscordRoleMapping(guildOperatorRoleId, Role.Operator),
        });

        SetupRefreshResponse();
        SetupLookupResponse(discordUser);
        SetupGuildMemberResponse(discordUser, guildOperatorRoleId);

        //Act
        var response = await client.PostAsync(
            "discord/renew",
            JsonContent.Create(new DiscordAuthenticationController.RenewRequest(refreshToken)));
        var Response = await response.Content.ReadAsJsonAsync<DiscordAuthenticationController.TokenResponse>();

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