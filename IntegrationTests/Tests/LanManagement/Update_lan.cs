using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Management.Lan;
using Xunit;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

// ReSharper disable once InconsistentNaming
public class Update_lan : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public Update_lan(TestWebApplicationFactory factory) : base(factory)
    {
    }

    public static IEnumerable<object[]> ValidUpdateRequestModels = new[]
    {
        new object[] { LanGenerator.UpdateLanRequestModel(id: Guid.NewGuid(), title: "updated title") },
        new object[]
        {
            LanGenerator.UpdateLanRequestModel(id: Guid.NewGuid(), background: LanGenerator.CreateValidBackround()),
        },
        new object[]
        {
            LanGenerator.UpdateLanRequestModel(
                id: Guid.NewGuid(),
                title: "updated title",
                background: LanGenerator.CreateValidBackround()),
        },
    };

    [Theory]
    [MemberData(nameof(ValidUpdateRequestModels))]
    public async Task succeeds_when_valid(LanController.UpdateLanRequestModel updateModel)
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);

        var existingLan = LanGenerator.CreateLan(updateModel.Id);
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
        new object[] { LanGenerator.UpdateLanRequestModel(id: Guid.NewGuid()) },
        new object[] { LanGenerator.UpdateLanRequestModel(id: Guid.NewGuid(), title: "") },
        new object[] { LanGenerator.UpdateLanRequestModel(id: Guid.NewGuid(), background: LanGenerator.InvalidBackround) },
    };

    [Theory]
    [MemberData(nameof(InvalidUpdateRequestModels))]
    public async Task fails_when_invalid(LanController.UpdateLanRequestModel updateModel)
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);

        var existingLan = LanGenerator.CreateLan(updateModel.Id);
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

        var existingLan = LanGenerator.CreateLan();
        SetupAggregates(existingLan);

        //Act
        var updateModel = LanGenerator.UpdateLanRequestModel(id: Guid.NewGuid(), background: LanGenerator.InvalidBackround);
        var response = await client.PutAsync(
             $"lan/{existingLan.Id}",
            JsonContent.Create(updateModel));

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}