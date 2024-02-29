using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints.Http.Authentication.Discord;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Authentication.Discord;

// ReSharper disable once InconsistentNaming
public class Login : LoginAndRenewBase
{
    public Login(TestWebApplicationFactory factory,
        PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    protected override Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId)
    {
        return client.PostAsync(
            "authentication/discord/login",
            JsonContent.Create(new LoginEndpoint.Request(DiscordToken, guildId, "https://local.host")));
    }


}