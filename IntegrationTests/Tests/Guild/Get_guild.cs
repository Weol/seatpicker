using System.Net;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Guild;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Guild;

public class Get_guild : IntegrationTestBase
{
    public Get_guild(TestWebApplicationFactory factory,
        PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId) =>
        await client.GetAsync($"guild/{guildId}");

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client) =>
        await client.GetAsync($"guild");

    [Fact]
    public async Task returns_all_guilds()
    {
        // Arrange
        var guildIds = new[]
        {
            await CreateGuild(),
            await CreateGuild(),
            await CreateGuild(),
        };

        var client = GetClient(guildIds[0], Role.Superadmin);

        //Act
        var response = await MakeRequest(client);
        var body = await response.Content.ReadAsJsonAsync<GetGuild.Response[]>();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().HaveCount(guildIds.Length);

        foreach (var guildId in guildIds)
        {
            body.Should().ContainSingle(guild => guild.Id == guildId);
        }
    }

    [Fact]
    public async Task returns_guild()
    {
        // Arrange
        var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Admin);

        //Act
        var response = await MakeRequest(client, guildId);
        var body = await response.Content.ReadAsJsonAsync<GetGuild.Response>();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();
        body!.Id.Should().Be(guildId);
    }
}