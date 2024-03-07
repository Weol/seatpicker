using System.Net;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Database.GuildHostMapping;
using Seatpicker.Infrastructure.Entrypoints.Http.Guild;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Guild;

// ReSharper disable once InconsistentNaming
[Collection("GuildHostMapping")]
public class Discover_guild : IntegrationTestBase
{
    public Discover_guild(
        TestWebApplicationFactory factory,
        PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper) : base(factory, databaseFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task retrieves_guild_id_when_mapping_is_set()
    {
        // Arrange
        var guild1 = CreateGuild();
        var guild2 = CreateGuild();
        var client1 = GetClient(guild1);
        client1.DefaultRequestHeaders.Host = "guild1.host1";
        var client2 = GetClient(guild2);
        client2.DefaultRequestHeaders.Host = "guild2.host1";

        await ClearDocumentsByType<GuildHostMapping>();
        await SetupDocuments(guild2, new GuildHostMapping("guild1.host1", guild1));
        await SetupDocuments(guild2, new GuildHostMapping("guild2.host1", guild2));

        //Act
        var response1 = await client1.GetAsync("guild/discover");
        var body1 = await response1.Content.ReadAsJsonAsync<DiscoverGuild.Response>();

        var response2 = await client2.GetAsync("guild/discover");
        var body2 = await response2.Content.ReadAsJsonAsync<DiscoverGuild.Response>();

        // Assert
        Assert.Multiple(
            () => response1.StatusCode.Should().Be(HttpStatusCode.OK),
            () => response2.StatusCode.Should().Be(HttpStatusCode.OK),
            () => body1!.GuildId.Should().Be(guild1),
            () => body2!.GuildId.Should().Be(guild2));
    }

    [Fact]
    public async Task returns_not_found_when_no_mapping_is_set()
    {
        // Arrange
        var guildId = CreateGuild();
        var client1 = GetClient(guildId);

        await ClearDocumentsByType<GuildHostMapping>();

        //Act
        var response = await client1.GetAsync("guild/discover");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}