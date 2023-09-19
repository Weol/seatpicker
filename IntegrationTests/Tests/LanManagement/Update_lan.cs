using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

// ReSharper disable once InconsistentNaming
public class Update_lan : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public Update_lan(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }

    public static IEnumerable<object[]> ValidUpdateRequestModels = new[]
    {
        new object[] { Generator.UpdateLanRequestModel() with { Title = null } },
        new object[]
        {
            Generator.UpdateLanRequestModel() with { Background = null },
        },
        new object[]
        {
            Generator.UpdateLanRequestModel(),
        },
    };

    [Theory]
    [MemberData(nameof(ValidUpdateRequestModels))]
    public async Task succeeds_when_valid(LanController.UpdateLanRequestModel updateModel)
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);

        var existingLan = LanGenerator.Create(updateModel.Id);
        SetupAggregates(existingLan);

        //Act
        var response = await client.PutAsJsonAsync($"lan/{updateModel.Id}", updateModel);

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
        new object[] { Generator.UpdateLanRequestModel() with { Background = null, Title = null } },
        new object[] { Generator.UpdateLanRequestModel() with { Title = "" } },
        new object[] { Generator.UpdateLanRequestModel() with { Background = Array.Empty<byte>() } },
        new object[] { Generator.UpdateLanRequestModel() with { Background = new byte[] { 1, 2, 3, 4 } } },
    };

    [Theory]
    [MemberData(nameof(InvalidUpdateRequestModels))]
    public async Task fails_when_invalid(LanController.UpdateLanRequestModel updateModel)
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);

        var existingLan = LanGenerator.Create(updateModel.Id);
        SetupAggregates(existingLan);

        //Act
        var response = await client.PutAsJsonAsync($"lan/{existingLan.Id}", updateModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task fails_when_model_id_does_not_match_path_id()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);

        var existingLan = LanGenerator.Create();
        SetupAggregates(existingLan);

        //Act
        var updateModel = Generator.UpdateLanRequestModel();
        var response = await client.PutAsJsonAsync($"lan/{existingLan.Id}", updateModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}