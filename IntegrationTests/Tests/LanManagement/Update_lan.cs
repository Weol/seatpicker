using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Application.Features.Lans;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.LanManagement;

// ReSharper disable once InconsistentNaming
public class Update_lan : IntegrationTestBase
{
    public Update_lan(TestWebApplicationFactory factory, PostgresFixture databaseFixture, ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }


    private async Task<HttpResponseMessage>
        MakeRequest(HttpClient client, Guid lanId, UpdateLan.Request request) =>
        await client.PutAsJsonAsync($"lan/{lanId}", request);

    public static IEnumerable<object[]> ValidUpdateRequests = new[]
    {
        new object[] { Generator.UpdateLanRequest() with { Title = null } },
        new object[] { Generator.UpdateLanRequest() with { Active = null } },
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
    public async Task succeeds_when_valid(UpdateLan.Request request)
    {
        // Arrange
        var client = GetClient(GuildId, Role.Admin);

        var existingLan = LanGenerator.Create(GuildId, request.Id);
        await SetupAggregates(GuildId, existingLan);

        //Act
        var response = await MakeRequest(client, existingLan.Id, request);

        //Assert
        var committedAggregates = GetCommittedDocuments<ProjectedLan>(GuildId);

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
    public async Task fails_when_invalid(UpdateLan.Request request)
    {
        // Arrange
        var client = GetClient(GuildId, Role.Admin);

        var existingLan = LanGenerator.Create(GuildId);
        await SetupAggregates(GuildId, existingLan);

        //Act
        var response = await MakeRequest(client, existingLan.Id, request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task fails_when_model_id_does_not_match_path_id()
    {
        // Arrange
        var client = GetClient(GuildId, Role.Admin);

        var existingLan = LanGenerator.Create(GuildId);
        await SetupAggregates(GuildId, existingLan);

        //Act
        var response = await MakeRequest(client, existingLan.Id, Generator.UpdateLanRequest());

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task fails_when_logged_in_user_has_insufficent_roles()
    {
        // Arrange
        var client = GetClient(GuildId);

        //Act
        var response = await MakeRequest(client, Guid.NewGuid(), Generator.UpdateLanRequest());

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}