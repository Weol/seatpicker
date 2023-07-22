using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Management.Lan;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

// ReSharper disable once InconsistentNaming
public class Update_lan : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public Update_lan(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
    {
    }

    public static IEnumerable<object[]> ValidUpdateRequestModels = new[]
    {
        new object[] { Generator.UpdateLanRequestModel(id: Guid.NewGuid(), title: "updated title") },
        new object[]
        {
            Generator.UpdateLanRequestModel(id: Guid.NewGuid(), background: AggregateGenerator.CreateValidBackround()),
        },
        new object[]
        {
            Generator.UpdateLanRequestModel(
                id: Guid.NewGuid(),
                title: "updated title",
                background: AggregateGenerator.CreateValidBackround()),
        },
    };

    [Theory]
    [MemberData(nameof(ValidUpdateRequestModels))]
    public async Task succeeds_when_valid(LanController.UpdateLanRequestModel updateModel)
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);

        var existingLan = AggregateGenerator.CreateLan(updateModel.Id);
        SetupAggregates(existingLan);

        //Act
        var response = await client.PutAsync(
            $"lan/{updateModel.Id}",
            JsonContent.Create(updateModel));

        //Assert
        var committedAggregates = GetCommittedAggregates<Lan>();

        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var lan = committedAggregates.Should().ContainSingle().Subject;
                Assert.Multiple(
                    () => lan.Id.Should().Be(updateModel.Id),
                    () =>
                    {
                        if (updateModel.Title is not null) lan.Title.Should().Be(updateModel.Title);
                    },
                    () =>
                    {
                        if (updateModel.Background is not null) lan.Background.Should().Equal(updateModel.Background);
                    });
            });
    }

    public static IEnumerable<object[]> InvalidUpdateRequestModels = new[]
    {
        new object[] { Generator.UpdateLanRequestModel(id: Guid.NewGuid()) },
        new object[] { Generator.UpdateLanRequestModel(id: Guid.NewGuid(), title: "") },
        new object[] { Generator.UpdateLanRequestModel(id: Guid.NewGuid(), background: Generator.InvalidBackround) },
    };

    [Theory]
    [MemberData(nameof(InvalidUpdateRequestModels))]
    public async Task fails_when_invalid(LanController.UpdateLanRequestModel updateModel)
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);

        var existingLan = AggregateGenerator.CreateLan(updateModel.Id);
        SetupAggregates(existingLan);

        //Act
        var response = await client.PutAsync(
             $"lan/{existingLan.Id}",
            JsonContent.Create(updateModel));

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task fails_model_id_does_not_match_path_id()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);

        var existingLan = AggregateGenerator.CreateLan();
        SetupAggregates(existingLan);

        //Act
        var updateModel = Generator.UpdateLanRequestModel(id: Guid.NewGuid(), background: Generator.InvalidBackround);
        var response = await client.PutAsync(
             $"lan/{existingLan.Id}",
            JsonContent.Create(updateModel));

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}