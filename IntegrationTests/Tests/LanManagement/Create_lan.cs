using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Domain;
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

        //Act
        var lanToCreate = AggregateGenerator.CreateLan();
        var response = await client.PostAsync("lan", JsonContent.Create(Generator.CreateLanRequestModel(lanToCreate)));

        //Assert
        var committedAggregates = GetCommittedAggregates<Lan>();

        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Created),
            () =>
            {
                var lan = committedAggregates.Should().ContainSingle().Subject;
                Assert.Multiple(
                    () => lan.Id.Should().Be(lanToCreate.Id),
                    () => lan.Title.Should().Be(lanToCreate.Title),
                    () => lan.Background.Should().Equal(lanToCreate.Background));
            });
    }

    [Fact]
    public async Task fails_when_lan_with_same_id_already_exists()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);

        var existingLan = AggregateGenerator.CreateLan(title: "Existing");
        var lanToCreate = AggregateGenerator.CreateLan(id: existingLan.Id, title: "Create");
        SetupAggregates(existingLan);

        //Act
        var response = await client.PostAsync("lan", JsonContent.Create(Generator.CreateLanRequestModel(lanToCreate)));

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

        var lanToCreate = AggregateGenerator.CreateLan(background: Generator.InvalidBackround);

        //Act
        var response = await client.PostAsync("lan", JsonContent.Create(Generator.CreateLanRequestModel(lanToCreate)));

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}