using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Infrastructure.Adapters.Database.GuildRoleMapping;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;
using Seatpicker.Infrastructure.Entrypoints.Http.Authentication.Discord;
using Seatpicker.IntegrationTests.HttpInterceptor.Discord;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Authentication;

// ReSharper disable once InconsistentNaming
public class Renew : LoginAndRenewBase 
{
    public Renew(TestWebApplicationFactory factory, PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    protected override Task<HttpResponseMessage> MakeRequest(HttpClient client)
    {
        return client.PostAsync(
            "authentication/discord/renew",
            JsonContent.Create(new RenewEndpoint.Request("refresh token", GuildId)));
    }
}