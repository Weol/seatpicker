using System.Diagnostics.CodeAnalysis;
using System.Net;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Guild;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Guild;

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class Get_guild(
    TestWebApplicationFactory fusery,
    PostgresFixture databaseFixture,
    ITestOutputHelper testOutputHelper) : IntegrationTestBase(fusery, databaseFixture, testOutputHelper)
{
    private static async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId) =>
        await client.GetAsync($"guild/{guildId}");

    private static async Task<HttpResponseMessage> MakeRequest(HttpClient client) =>
        await client.GetAsync($"guild");

    [Fact]
    public async Task returns_all_guilds()
    {
        // Arrange
        var guilds = new[]
        {
            await CreateGuild(),
            await CreateGuild(),
            await CreateGuild(),
        };

        var client = GetClient(guilds[0].Id, Role.Superadmin);

        // Act
        var response = await MakeRequest(client);
        var body = await response.Content.ReadAsJsonAsync<GetGuild.Response[]>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().HaveCount(guilds.Length);

        foreach (var guild in guilds)
        {
            body.Should().ContainSingle(responseGuild => guild.Id == responseGuild.Id);
        }
    }

    [Fact]
    public async Task returns_guild()
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Admin);

        // Act
        var response = await MakeRequest(client, guild.Id);
        var body = await response.Content.ReadAsJsonAsync<GetGuild.Response>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();
        body!.Id.Should().Be(guild.Id);
    }
}