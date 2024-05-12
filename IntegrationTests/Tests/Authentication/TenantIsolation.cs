using System.Net;
using FluentAssertions;
using Seatpicker.Domain;
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
		var guildId = await CreateGuild();
        var client = GetClient(guildId);

        // Act
        var response = await client.GetAsync($"authentication/test/123{guildId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task request_succeeds_when_jwt_guild_and_guildId_in_route_are_the_same()
    {
        // Arrange
		var guildId = await CreateGuild();
        var client = GetClient(guildId);

        // Act
        var response = await client.GetAsync($"authentication/test/{guildId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task request_succeeds_when_jwt_has_no_guild_id_and_is_superadnim()
    {
        // Arrange
		var guildId = await CreateGuild();
        var identity = await CreateIdentity(guildId, new[] { Role.Superadmin }, true);
        var client = GetClient(identity);

        // Act
        var response = await client.GetAsync($"authentication/test/{guildId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task request_fails_when_jwt_has_no_guild_id_and_is_not_superadmin()
    {
        // Arrange
		var guildId = await CreateGuild();
        var identity = await CreateIdentity(guildId, new[] { Role.Superadmin }, true);
        var client = GetClient(identity);

        // Act
        var response = await client.GetAsync($"authentication/test/{guildId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}