using System.Net;
using FluentAssertions;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

// ReSharper disable once InconsistentNaming
public class Get_lan : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public Get_lan(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }

    [Fact]
    public async Task returns_lan_when_lan_exists()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);

        var existingLan = LanGenerator.Create();
        SetupAggregates(existingLan);

        //Act
        var request = await client.GetAsync($"lan/{existingLan.Id}");
        var response = await request.Content.ReadAsJsonAsync<GetEndpoint.Response>();

        //Assert
        Assert.Multiple(
            () => request.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                response.Should().NotBeNull();
                Assert.Multiple(
                    () => response!.Id.Should().Be(existingLan.Id),

                    () => response!.Title.Should().Be(existingLan.Title),
                    () => response!.Background.Should().Equal(existingLan.Background));
            });
    }

    [Fact]
    public async Task returns_nothing_when_lan_does_not_exist()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);

        //Act
        var response = await client.GetAsync($"lan/{Guid.NewGuid()}");
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}