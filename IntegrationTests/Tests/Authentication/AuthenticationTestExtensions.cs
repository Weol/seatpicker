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
public static class AuthenticationTestExtensions
{
    public static void SetupRefreshResponse(this IntegrationTestBase integrationTestBase)
    {
        integrationTestBase.MockOutgoingHttpRequest(
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

    public static void SetupAccessTokenResponse(this IntegrationTestBase integrationTestBase)
    {
        integrationTestBase.MockOutgoingHttpRequest(
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

    public static void SetupLookupResponse(this IntegrationTestBase integrationTestBase, DiscordUser discordUser)
    {
        integrationTestBase.MockOutgoingHttpRequest(
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

    public static void SetupGuildMemberResponse(this IntegrationTestBase integrationTestBase, DiscordUser discordUser, params string[] guildRoles)
    {
        var roles = string.Join(",", guildRoles.Select(role => "\"" + role + "\""));

        integrationTestBase.MockOutgoingHttpRequest(
            request => request.RequestUri!.ToString().EndsWith($"members/{discordUser.Id}"),
            _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    $$"""
                        {
                          "id": "{{discordUser.Id}}",
                          "avatar": {{discordUser.Avatar ?? "null"}},
                          "roles": [{{roles}}],
                          "user": {
                            "id": "{{discordUser.Id}}",
                            "username": "{{discordUser.Username}}",
                            "discriminator": "1337",
                            "avatar": {{discordUser.Avatar ?? "null"}}
                          }
                        }
                    """),
            });
    }

    public static void SetupRolesResponse(this IntegrationTestBase integrationTestBase, string operatorId)
    {
        integrationTestBase.MockOutgoingHttpRequest(
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
}