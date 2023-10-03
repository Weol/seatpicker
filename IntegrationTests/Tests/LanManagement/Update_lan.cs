using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication;
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

    public static IEnumerable<object[]> ValidUpdateRequests = new[]
    {
        new object[] { Generator.UpdateLanRequest() with { Title = null } },
        new object[]
        {
            Generator.UpdateLanRequest() with { Background = null },
        },
        new object[]
        {
            Generator.UpdateLanRequest(),
        },
    };

    [Theory]
    [MemberData(nameof(ValidUpdateRequests))]
    public async Task succeeds_when_valid(Update.Request request)
    {
        // Arrange
        var identity = await CreateIdentity(Role.Admin);
        var client = GetClient(identity);

        var existingLan = LanGenerator.Create(request.Id);
        SetupAggregates(existingLan);

        //Act
        var response = await client.PutAsJsonAsync($"lan/{request.Id}", request);

        //Assert
        var committedAggregates = GetCommittedAggregates<Lan>();

        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var lan = committedAggregates.Should().ContainSingle().Subject;
                Assert.Multiple(
                    () => lan.Id.Should().Be(request.Id),
                    () =>
                    {
                        if (request.Title is not null) lan.Title.Should().Be(request.Title);
                    },
                    () =>
                    {
                        if (request.Background is not null) lan.Background.Should().Equal(request.Background);
                    });
            });
    }

    public static IEnumerable<object[]> InvalidUpdateRequests = new[]
    {
        new object[] { Generator.UpdateLanRequest() with { Background = null, Title = null } },
        new object[] { Generator.UpdateLanRequest() with { Title = "" } },
        new object[] { Generator.UpdateLanRequest() with { Background = Array.Empty<byte>() } },
        new object[] { Generator.UpdateLanRequest() with { Background = new byte[] { 1, 2, 3, 4 } } },
    };

    [Theory]
    [MemberData(nameof(InvalidUpdateRequests))]
    public async Task fails_when_invalid(Update.Request updateModel)
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
        var updateModel = Generator.UpdateLanRequest();
        var response = await client.PutAsJsonAsync($"lan/{existingLan.Id}", updateModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}