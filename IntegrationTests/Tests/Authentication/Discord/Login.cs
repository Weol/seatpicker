using System.Net.Http.Json;
using Seatpicker.Infrastructure.Entrypoints.Http.Authentication.Discord;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Authentication.Discord;

// ReSharper disable once InconsistentNaming
public class Login(
    TestWebApplicationFactory fusery,
    PostgresFixture databaseFixture,
    ITestOutputHelper testOutputHelper) : LoginAndRenewBase(fusery, databaseFixture, testOutputHelper)
{
    protected override Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId)
    {
        return client.PostAsync(
            "authentication/discord/login",
            JsonContent.Create(new LoginEndpoint.Request(DiscordToken, guildId, "https://local.host")));
    }


}