using System.Net.Http.Json;
using Seatpicker.Infrastructure.Entrypoints.Http.Authentication.Discord;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Authentication.Discord;

// ReSharper disable once InconsistentNaming
public class Renew(
    TestWebApplicationFactory factory,
    PostgresFixture databaseFixture,
    ITestOutputHelper testOutputHelper) : LoginAndRenewBase(factory, databaseFixture, testOutputHelper)
{
    protected override Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId)
    {
        return client.PostAsync(
            "authentication/discord/renew",
            JsonContent.Create(new RenewEndpoint.Request(RefreshToken, guildId)));
    }
}