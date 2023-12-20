using System.Net;
using FluentAssertions;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

// ReSharper disable once InconsistentNaming
public class Get_lan : IntegrationTestBase
{
    public Get_lan(TestWebApplicationFactory factory, PostgresFixture databaseFixture, ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client, Guid lanId) =>
        await client.GetAsync($"lan/{lanId}");

    [Fact]
    public async Task returns_lan_when_lan_exists()
    {
        // Arrange
        var client = GetClient();

        var existingLan = LanGenerator.Create(GuildId);
        await SetupAggregates(existingLan);

        //Act
        var response = await MakeRequest(client, existingLan.Id);
        var body = await response.Content.ReadAsJsonAsync<GetEndpoint.Response>();

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                body.Should().NotBeNull();
                Assert.Multiple(
                    () => body!.Id.Should().Be(existingLan.Id),
                    () => body!.Title.Should().Be(existingLan.Title),
                    () => body!.Background.Should().Equal(existingLan.Background));
            });
    }

    [Fact]
    public async Task returns_nothing_when_lan_does_not_exist()
    {
        // Arrange
        var client = GetClient();

        //Act
        var response = await MakeRequest(client, Guid.NewGuid());

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}