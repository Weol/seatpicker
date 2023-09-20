using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication.Discord;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;
using Seatpicker.Infrastructure.Entrypoints.Http;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests;

// ReSharper disable once InconsistentNaming
public class Renew : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
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

        this.SetupRefreshResponse();
        this.SetupLookupResponse(discordUser);
        this.SetupGuildMemberResponse(discordUser);

        //Act
        var response = await client.PostAsync(
            "discord/renew",
            JsonContent.Create(new DiscordAuthenticationController.RenewRequestModel(refreshToken)));
        var responseModel = await response.Content.ReadFromJsonAsync<DiscordAuthenticationController.TokenResponseModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseModel.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", responseModel!.Token);
        var testResponse = await client.GetAsync("discord/test");
        var testResponseModel
            = await testResponse.Content.ReadFromJsonAsync<DiscordAuthenticationController.TestResponseModel>();

        testResponseModel.Should().NotBeNull();

        Assert.Multiple(
            () => testResponseModel!.Id.Should().Be(discordUser.Id),
            () => testResponseModel!.Name.Should().Be(discordUser.Username),
            () => testResponseModel!.Roles.Should().Contain(Role.User.ToString()));
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
        await mapper.Set(new[]
        {
            new DiscordRoleMapping(guildOperatorRoleId, Role.Operator),
        });

        this.SetupRefreshResponse();
        this.SetupLookupResponse(discordUser);
        this.SetupGuildMemberResponse(discordUser, guildOperatorRoleId);

        //Act
        var response = await client.PostAsync(
            "discord/renew",
            JsonContent.Create(new DiscordAuthenticationController.RenewRequestModel(refreshToken)));
        var responseModel = await response.Content.ReadFromJsonAsync<DiscordAuthenticationController.TokenResponseModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseModel.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", responseModel!.Token);
        var testResponse = await client.GetAsync("discord/test");
        var testResponseModel
            = await testResponse.Content.ReadFromJsonAsync<DiscordAuthenticationController.TestResponseModel>();

        testResponseModel.Should().NotBeNull();

        Assert.Multiple(
            () => testResponseModel!.Id.Should().Be(discordUser.Id),
            () => testResponseModel!.Name.Should().Be(discordUser.Username),
            () => testResponseModel!.Roles.Should().Contain(Role.User.ToString()),
            () => testResponseModel!.Roles.Should().Contain(Role.Operator.ToString()));
    }
}