using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Seatpicker.Infrastructure.Adapters.DiscordClient;
using Seatpicker.Infrastructure.Authentication.Discord;
using Seatpicker.Infrastructure.Entrypoints.Http;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests;

// ReSharper disable once InconsistentNaming
public class DiscordLogin : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public DiscordLogin(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }

    private void SetupRefreshResponse()
    {
        MockOutgoingHttpRequest(
                request => request.RequestUri!.ToString().EndsWith("oauth2/token"),
                _ => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                            "access_token": "9283hfaojflsjjdskjfa3ijdsfasfd",
                            "token_type": "Bearer",
                            "expires_in": 604800,
                            "refresh_token": "dfafjajfoijadsløfjaaøeoi32",
                            "scope": "identify"
                        }
                    """),
                });
    }

    private void SetupAccessTokenResponse()
    {
        MockOutgoingHttpRequest(
            request => request.RequestUri!.ToString().EndsWith("oauth2/token"),
            _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                        {
                            "access_token": "auiywbfuiksdflkaelkiuadsf",
                            "token_type": "Bearer",
                            "expires_in": 604800,
                            "refresh_token": "asdoi32hdkah3kiafwa3fadf",
                            "scope": "identify"
                        }
                    """),
            });
    }

    private void SetupLookupResponse(DiscordUser discordUser)
    {
        MockOutgoingHttpRequest(
            request => request.RequestUri!.ToString().EndsWith("users/@me"),
            _ => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        $$"""
                        {
                          "id": "{{discordUser.Id}}",
                          "username": "{{discordUser.Username}}",
                          "discriminator": "1337",
                          "avatar": {{discordUser.Avatar ?? "null"}}
                        }
                    """),
                });
    }

    private ClaimsPrincipal DeserializeJwtToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var validations = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = false,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            SignatureValidator = (t, _) => new JwtSecurityTokenHandler().ReadToken(t),
        };
        return handler.ValidateToken(token, validations, out _);
    }

    [Fact]
    public async Task acquire_succeeds_when_discord_token_is_valid()
    {
        // Arrange
        var client = GetAnonymousClient();
        var token = "discord token";

        var discordUser = new DiscordUser("123", "Tore Tang", null);

        SetupAccessTokenResponse();
        SetupLookupResponse(discordUser);

        //Act
        var response = await client.PostAsync(
            "discord/login",
            JsonContent.Create(new DiscordLoginEndpoints.TokenRequestModel(token)));
        var responseModel = await response.Content.ReadFromJsonAsync<DiscordLoginEndpoints.TokenResponseModel>();

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                responseModel.Should().NotBeNull();
                var claimsPrincipal = DeserializeJwtToken(responseModel!.Token);
                var identity = claimsPrincipal.Identity as ClaimsIdentity;
                identity.Should().NotBeNull();
                identity!.Name.Should().Be(discordUser.Username);
                identity.Claims.Should()
                    .ContainSingle(claim => claim.Type == ClaimTypes.NameIdentifier && claim.Value == discordUser.Id);
            });
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

        //Act
        var response = await client.PostAsync(
            "discord/renew",
            JsonContent.Create(new DiscordLoginEndpoints.TokenRenewModel(refreshToken)));
        var responseModel = await response.Content.ReadFromJsonAsync<DiscordLoginEndpoints.TokenResponseModel>();

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                responseModel.Should().NotBeNull();
                var claimsPrincipal = DeserializeJwtToken(responseModel!.Token);
                var identity = claimsPrincipal.Identity as ClaimsIdentity;
                identity.Should().NotBeNull();
                identity!.Name.Should().Be(discordUser.Username);
                identity.Claims.Should()
                    .ContainSingle(claim => claim.Type == ClaimTypes.NameIdentifier && claim.Value == discordUser.Id);
            });
    }
}