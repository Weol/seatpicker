using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Management.Lan;
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

        var existingLan = AggregateGenerator.CreateLan();
        SetupAggregates(existingLan);

        //Act
        var response = await client.GetAsync($"lan/{existingLan.Id}");
        var responseModel = await response.Content.ReadFromJsonAsync<LanController.LanResponseModel>();

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                responseModel.Should().NotBeNull();
                Assert.Multiple(
                    () => responseModel!.Id.Should().Be(existingLan.Id),
                    () => responseModel!.Title.Should().Be(existingLan.Title),
                    () => responseModel!.Background.Should().Be(Convert.ToBase64String(existingLan.Background)));
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