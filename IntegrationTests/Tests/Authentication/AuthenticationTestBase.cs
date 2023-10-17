using System.Net;
using System.Net.Http.Headers;
using Seatpicker.Infrastructure.Authentication.Discord;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;
using Seatpicker.Infrastructure.Entrypoints.Http.DiscordAuthentication;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Authentication;

// ReSharper disable once InconsistentNaming
public class AuthenticationTestBase : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    protected AuthenticationTestBase(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }

    protected void SetupRefreshResponse()
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

    protected void SetupAccessTokenResponse()
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

    protected void SetupLookupResponse(DiscordUser discordUser)
    {
        MockOutgoingHttpRequest(
            request => request.RequestUri!.ToString().EndsWith("users/@me"),
            _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    $$"""
                        {
                          "id": "{{ discordUser.Id}}",
                          "username": "{{ discordUser.Username}}",
                          "discriminator": "1337",
                          "avatar": {{ discordUser.Avatar ?? "null"}}
                        }
                    """ ),
            });
    }

    protected void SetupGuildMemberResponse(DiscordUser discordUser, params string[] guildRoles)
    {
        var roles = string.Join(",", guildRoles.Select(role => "\"" + role + "\""));

        MockOutgoingHttpRequest(
            request => request.RequestUri!.ToString().EndsWith($"members/{discordUser.Id}"),
            _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    $$"""
                        {
                          "id": "{{ discordUser.Id}}",
                          "avatar": {{ discordUser.Avatar ?? "null"}}  ,
                          "roles": [{{ roles}}  ],
                          "user": {
                            "id": "{{ discordUser.Id}}",
                            "username": "{{ discordUser.Username}}",
                            "discriminator": "1337",
                            "avatar": {{ discordUser.Avatar ?? "null"}}
                          }
                        }
                    """ ),
            });
    }

    protected void SetupRolesResponse(string operatorId)
    {
        MockOutgoingHttpRequest(
            request => request.RequestUri!.ToString().EndsWith("roles"),
            _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    $$"""
                        [
                          {
                            "id": "1234567",
                            "name": "@everyone",
                            "color": 123
                          },
                          {
                            "id": "{{operatorId}}",
                            "name": "Operator",
                            "color": 213
                          }
                        ]
                    """),
            });
    }

    protected static async Task<DiscordAuthenticationController.TestResponse> TestAuthentication(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync("discord/test");
        var Response = await response.Content.ReadAsJsonAsync<DiscordAuthenticationController.TestResponse>();

        if (Response is null) throw new NullReferenceException();

        return Response;
    }
}