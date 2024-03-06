using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.Database.GuildHostMapping;
using Seatpicker.Infrastructure.Adapters.Database.GuildRoleMapping;
using Seatpicker.Infrastructure.Entrypoints.Http.Guild;
using Seatpicker.Infrastructure.Entrypoints.Http.Guild.Discord;
using Seatpicker.IntegrationTests.TestAdapters;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Guild;

// ReSharper disable once InconsistentNaming
public class Host_mapping : IntegrationTestBase
{
    public Host_mapping(TestWebApplicationFactory factory,
        PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    [Fact]
    public async Task getting_host_mapping_succeeds()
    {
        // Arrange
        var guild1 = CreateGuild();
        var guild2 = CreateGuild();
        var client = GetClient(guild2, Role.Superadmin);

        await SetupDocuments(guild2,
            new GuildHostMapping(guild1, "guild1.host1"),
            new GuildHostMapping(guild1, "guild1.host2"));
        await SetupDocuments(guild2, new GuildHostMapping(guild2, "guild2.host1"));

        //Act
        var response = await client.GetAsync("guild/hosts");
        var body = await response.Content.ReadAsJsonAsync<IEnumerable<GetHostMapping.Response>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().HaveCount(2);

        Assert.Multiple(
            () =>
            {
                var mapping = body.Should().ContainSingle(x => x.GuildId == guild1).Subject;
                mapping.Hostnames.Should().HaveCount(2);
                mapping.Hostnames.Should().ContainSingle(x => x == "guild1.host1");
                mapping.Hostnames.Should().ContainSingle(x => x == "guild1.host2");
            },
            () =>
            {
                var mapping = body.Should().ContainSingle(x => x.GuildId == guild2).Subject;
                mapping.Hostnames.Should().HaveCount(1);
                mapping.Hostnames.Should().ContainSingle(x => x == "guild2.host1");
            });
    }
}