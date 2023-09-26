using System.Net;
using FluentAssertions;
using Seatpicker.Domain;
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
        var response = await client.GetAsync($"lan/{existingLan.Id}");
        var Response = await response.Content.ReadAsJsonAsync<LanController.LanResponse>();

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                Response.Should().NotBeNull();
                Assert.Multiple(
                    () => Response!.Id.Should().Be(existingLan.Id),

                    () => Response!.Title.Should().Be(existingLan.Title),
                    () => Response!.Background.Should().Be(Convert.ToBase64String(existingLan.Background)));
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