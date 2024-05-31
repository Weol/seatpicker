using System.Diagnostics.CodeAnalysis;
using System.Net;
using Bogus;
using FluentAssertions;
using Seatpicker.Infrastructure.Adapters.Guilds;
using Seatpicker.Infrastructure.Entrypoints.Http.Frontend;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Guild;

// ReSharper disable once InconsistentNaming
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
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
        var guild1 = await CreateGuild(RandomData.Guild() with { Hostnames = new []{ "guild1.host1" }});
        var guild2 = await CreateGuild(RandomData.Guild() with { Hostnames = new []{ "guild2.host1" }});
        var client1 = GetClient(guild1.Id);
        client1.DefaultRequestHeaders.Host = "guild1.host1";
        var client2 = GetClient(guild2.Id);
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
            () => body1!.GuildId.Should().Be(guild1.Id),
            () => body2!.GuildId.Should().Be(guild2.Id),
            () => body1!.Lan.Should().BeNull(),
            () => body2!.Lan.Should().BeNull());
    }

    [Fact]
    public async Task retrieves_guild_id_and_active_lan_when_both_are_set()
    {
        // Arrange
        var guild = await CreateGuild(RandomData.Guild() with { Hostnames = new []{ "guild3.host1" }});
        var client1 = GetClient(guild.Id);
        client1.DefaultRequestHeaders.Host = "guild3.host1";

        var initiator = CreateUser(guild.Id);
        var activeLan = RandomData.Aggregates.Lan(guild.Id, initiator);
        activeLan.SetActive(true, initiator);
        await SetupAggregates(guild.Id, activeLan);

        // Act
        var response = await client1.GetAsync("guild/discover");
        var body = await response.Content.ReadAsJsonAsync<Discover.Response>();

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () => body!.GuildId.Should().Be(guild.Id),
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
        var guild = await CreateGuild();
        var client = GetClient(guild.Id);
        client.DefaultRequestHeaders.Host = RandomData.Hostname();

        // Act
        var response = await client.GetAsync("guild/discover");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}