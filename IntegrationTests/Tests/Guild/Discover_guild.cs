using System.Net;
using Bogus;
using FluentAssertions;
using Seatpicker.Infrastructure.Adapters.Guilds;
using Seatpicker.Infrastructure.Entrypoints.Http.Frontend;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Guild;

// ReSharper disable once InconsistentNaming
public class Discover_guild : IntegrationTestBase
{
    public Discover_guild(
        TestWebApplicationFactory factory,
        PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper) : base(factory, databaseFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task retrieves_only_guild_id_when_host_mapping_is_set_but_no_active_lan_is_set()
    {
        // Arrange
        var guild1 = await CreateGuild(hostnames: new [] { "guild1.host1" });
        var guild2 = await CreateGuild(hostnames: new [] { "guild2.host1" });
        var client1 = GetClient(guild1);
        client1.DefaultRequestHeaders.Host = "guild1.host1";
        var client2 = GetClient(guild2);
        client2.DefaultRequestHeaders.Host = "guild2.host1";

        // Act
        var response1 = await client1.GetAsync("guild/discover");
        var body1 = await response1.Content.ReadAsJsonAsync<Discover.Response>();

        var response2 = await client2.GetAsync("guild/discover");
        var body2 = await response2.Content.ReadAsJsonAsync<Discover.Response>();

        // Assert
        Assert.Multiple(
            () => response1.StatusCode.Should().Be(HttpStatusCode.OK),
            () => response2.StatusCode.Should().Be(HttpStatusCode.OK),
            () => body1!.GuildId.Should().Be(guild1),
            () => body2!.GuildId.Should().Be(guild2),
            () => body1!.Lan.Should().BeNull(),
            () => body2!.Lan.Should().BeNull());
    }

    [Fact]
    public async Task retrieves_guild_id_and_active_lan_when_both_are_set()
    {
        // Arrange
        var guildId = await CreateGuild();
        var client1 = GetClient(guildId);
        client1.DefaultRequestHeaders.Host = "guild3.host1";

        await SetupDocuments(new GuildAdapter.GuildDocument(guildId,
            "Name",
            null,
            new[] { "guild3.host1" },
            Array.Empty<GuildAdapter.GuildRoleMapping>()));

        var initiator = CreateUser(guildId);
        var activeLan = LanGenerator.Create(guildId, initiator);
        activeLan.SetActive(true, initiator);
        await SetupAggregates(guildId, activeLan);

        // Act
        var response = await client1.GetAsync("guild/discover");
        var body = await response.Content.ReadAsJsonAsync<Discover.Response>();

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () => body!.GuildId.Should().Be(guildId),
            () =>
            {
                body!.Lan.Should().NotBeNull();
                Assert.Multiple(
                    () => body.Lan!.Id.Should().Be(activeLan.Id),
                    () => body.Lan!.Title.Should().Be(activeLan.Title),
                    () => body.Lan!.Background.Should().Equal(activeLan.Background),
                    () => body.Lan!.Active.Should().Be(activeLan.Active));
            });
    }

    [Fact]
    public async Task returns_not_found_when_no_mapping_is_set()
    {
        // Arrange
        var guildId = await CreateGuild();
        var client = GetClient(guildId);
        client.DefaultRequestHeaders.Host = new Faker().Random.Word() + ".com";

        // Act
        var response = await client.GetAsync("guild/discover");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}