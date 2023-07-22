using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Seatpicker.Application.Features.Token;
using Seatpicker.Infrastructure.Entrypoints.Http;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests;

// ReSharper disable once InconsistentNaming
public class Token : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public Token(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }

    private void SetupDiscordResponses(DiscordUser discordUser)
    {
        InterceptingHttpMessageHandler
            .Handle(Arg.Is<HttpRequestMessage>(h => h.RequestUri!.ToString().EndsWith("oauth2/token")))
            .Returns(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                            "access_token": "6qrZcUqja7812RVdnEKjpzOL4CvHBFG",
                            "token_type": "Bearer",
                            "expires_in": 604800,
                            "refresh_token": "D43f5y0ahjqew82jZ4NViEr2YafMKhue",
                            "scope": "identify"
                        }
                    """),
                });

        InterceptingHttpMessageHandler
            .Handle(Arg.Is<HttpRequestMessage>(h => h.RequestUri!.ToString().EndsWith("users/@me")))
            .Returns(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        $$"""
                        {
                          "id": " {{ discordUser.Id}} ",
                          "username": "{{ discordUser.Username}} ",
                          "discriminator": "1337",
                          "avatar": {{ discordUser.Avatar ?? "null"}}
                        }
                    """ ),
                });
    }

    private Task<IEnumerable<Claim>> DeserializeJwtToken(string token)
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
        var claims = handler.ValidateToken(token, validations, out var _);
        return Task.FromResult(claims.Claims);
    }

    [Fact]
    public async Task acquire_succeeds_when_discord_token_is_valid()
    {
        // Arrange
        var client = GetAnonymousClient();
        var token = "discord token";

        var discordUser = new DiscordUser("123", "Tore Tang", null);

        SetupDiscordResponses(discordUser);

        //Act
        var response = await client.PostAsync(
            "token",
            JsonContent.Create(new TokenController.TokenRequestModel(token)));
        var responseModel = await response.Content.ReadFromJsonAsync<TokenController.TokenResponseModel>();

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            async () =>
            {
                responseModel.Should().NotBeNull();
                var claims = await DeserializeJwtToken(responseModel!.Token);
                Assert.Multiple(
                    () => claims.Should()
                        .ContainSingle(claim => claim.Type == "spu_id" && claim.Value == discordUser.Id),
                    () => claims.Should()
                        .ContainSingle(claim => claim.Type == "spu_nick" && claim.Value == discordUser.Username),
                    () => claims.Should()
                        .ContainSingle(claim => claim.Type == "spu_avatar" && claim.Value == discordUser.Avatar));
            });
    }
}