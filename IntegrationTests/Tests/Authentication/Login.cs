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
public class Login : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
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

        this.SetupAccessTokenResponse();
        this.SetupLookupResponse(discordUser);
        this.SetupGuildMemberResponse(discordUser);

        //Act
        var response = await client.PostAsync(
            "discord/login",
            JsonContent.Create(new DiscordAuthenticationController.TokenRequestModel(token)));
        var responseModel
            = await response.Content.ReadFromJsonAsync<DiscordAuthenticationController.TokenResponseModel>();

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
    public async Task login_succeeds_when_discord_token_is_valid_and_has_roles_according_to_mapping()
    {
        // Arrange
        var client = GetAnonymousClient();
        var token = "discord token";

        var discordUser = new DiscordUser("123", "Tore Tang", null);

        var guildOperatorRoleId = "999999";

        var mapper = GetService<DiscordRoleMapper>();
        await mapper.Set(new[]
        {
            new DiscordRoleMapping(guildOperatorRoleId, Role.Operator),
        });

        this.SetupAccessTokenResponse();
        this.SetupLookupResponse(discordUser);
        this.SetupGuildMemberResponse(discordUser, guildOperatorRoleId);

        //Act
        var response = await client.PostAsync(
            "discord/login",
            JsonContent.Create(new DiscordAuthenticationController.TokenRequestModel(token)));
        var responseModel
            = await response.Content.ReadFromJsonAsync<DiscordAuthenticationController.TokenResponseModel>();

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