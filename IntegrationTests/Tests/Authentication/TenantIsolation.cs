using System.Diagnostics.CodeAnalysis;
using System.Net;
using FluentAssertions;
using Seatpicker.Domain;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Authentication;

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class TenantIsolation(
    TestWebApplicationFactory factory,
    PostgresFixture databaseFixture,
    ITestOutputHelper testOutputHelper) : IntegrationTestBase(factory, databaseFixture, testOutputHelper)
{
    [Fact]
    public async Task request_denied_when_there_is_mismatch_between_jwt_guild_and_guildId_in_route()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id);

        // Act
        var response = await client.GetAsync($"authentication/test/123{guild.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task request_succeeds_when_jwt_guild_and_guildId_in_route_are_the_same()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id);

        // Act
        var response = await client.GetAsync($"authentication/test/{guild.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task request_succeeds_when_jwt_has_no_guild_id_and_is_superadnim()
    {
        // Arrange
		var guild = await CreateGuild();
        var identity = await CreateIdentity(guild.Id, [Role.Superadmin], true);
        var client = GetClient(identity);

        // Act
        var response = await client.GetAsync($"authentication/test/{guild.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task request_fails_when_jwt_has_no_guild_id_and_is_not_superadmin()
    {
        // Arrange
		var guild = await CreateGuild();
        var identity = await CreateIdentity(guild.Id, [Role.Admin], true);
        var client = GetClient(identity);

        // Act
        var response = await client.GetAsync($"authentication/test/{guild.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}