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
    public async Task request_denied_when_there_is_mismatch_between_jwt_guild_and_tenant_header()
    {
        // Arrange
		var guildId = CreateGuild(); 
        var client = GetClient("321");

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "authentication/test");
        var response = await client.SendAsync(request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    
    [Fact]
    public async Task request_succeeds_when_jwt_guild_and_tenant_header_are_the_same()
    {
        // Arrange
		var guildId = CreateGuild(); 
        var client = GetClient(guildId);

        // Act
        var response = await client.GetAsync("authentication/test");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}