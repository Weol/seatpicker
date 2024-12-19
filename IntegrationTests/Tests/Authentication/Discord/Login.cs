using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using Seatpicker.Infrastructure.Entrypoints.Http.Authentication.Discord;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Authentication.Discord;

// ReSharper disable once InconsistentNaming
[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class Login(
    TestWebApplicationFactory factory,
    PostgresFixture databaseFixture,
    ITestOutputHelper testOutputHelper) : LoginAndRenewBase(factory, databaseFixture, testOutputHelper)
{
    protected override Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId)
    {
        return client.PostAsync(
            "authentication/discord/login",
            JsonContent.Create(new LoginEndpoint.Request(DiscordToken, guildId, "https://local.host")));
    }


}