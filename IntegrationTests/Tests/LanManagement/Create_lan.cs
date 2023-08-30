using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

// ReSharper disable once InconsistentNaming
public class Create_lan : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public Create_lan(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }

    [Fact]
    public async Task succeeds_when_lan_is_valid()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);

        var requestModel = Generator.CreateLanRequestModel();

        //Act
        var response = await client.PostAsJsonAsync("lan", requestModel);

        //Assert
        var committedAggregates = GetCommittedAggregates<Lan>();

        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Created),
            () =>
            {
                var lan = committedAggregates.Should().ContainSingle().Subject;
                Assert.Multiple(
                    () => lan.Id.Should().Be(requestModel.Id),
                    () => lan.Title.Should().Be(requestModel.Title),
                    () => lan.Background.Should().Equal(requestModel.Background));
            });
    }

    [Fact]
    public async Task fails_when_lan_with_same_id_already_exists()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);

        var existingLan = LanGenerator.Create(title: "Existing");
        SetupAggregates(existingLan);

        //Act
        var response = await client.PostAsJsonAsync(
            "lan",
            Generator.CreateLanRequestModel() with { Id = existingLan.Id });

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Conflict),
            () =>
            {
                var lan = GetCommittedAggregates<Lan>().Should().ContainSingle().Subject;
                lan.Title.Should().Be(existingLan.Title);
            });
    }

    [Fact]
    public async Task fails_when_background_is_not_svg()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);

        //Act
        var response = await client.PostAsJsonAsync(
            "lan",
            Generator.CreateLanRequestModel() with { Background = new byte[] { 1, 2, 3, 4, 5, 6 } });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}