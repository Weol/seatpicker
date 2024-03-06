using System.Net;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Authentication;

public class TenantIsolation : IntegrationTestBase
{
    public TenantIsolation(TestWebApplicationFactory factory,
        PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    [Fact]
    public async Task request_denied_when_there_is_mismatch_between_jwt_guild_and_guildId_in_route()
    {
        // Arrange
		var guildId = CreateGuild();
        var client = GetClient(CreateGuild());

        // Act
        var response = await client.GetAsync($"authentication/test/{guildId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task request_succeeds_when_jwt_guild_and_guildId_in_route_are_the_same()
    {
        // Arrange
		var guildId = CreateGuild();
        var client = GetClient(guildId);

        // Act
        var response = await client.GetAsync($"authentication/test/{guildId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}