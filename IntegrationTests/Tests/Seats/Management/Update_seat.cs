using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints.Http.Seat;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.Management;

// ReSharper disable once InconsistentNaming
public class Update_seat : IntegrationTestBase
{
    public Update_seat(TestWebApplicationFactory factory, PostgresFixture databaseFixture, ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client, Guid lanId, Guid seatId,
        UpdateEndpoint.Request request) =>
        await client.PutAsJsonAsync($"lan/{lanId}/seat/{seatId}", request);

    public static IEnumerable<object[]> ValidUpdateRequests = new[]
    {
        new object[] { Generator.UpdateSeatRequest() },
        new object[]
        {
            Generator.UpdateSeatRequest() with { Title = null },
        },
        new object[]
        {
            Generator.UpdateSeatRequest() with { Bounds = null },
        },
    };

    [Theory]
    [MemberData(nameof(ValidUpdateRequests))]
    public async Task succeeds_when_valid(UpdateEndpoint.Request request)
    {
        // Arrange
        var client = GetClient(GuildId, Role.Operator);

        var lan = LanGenerator.Create(GuildId);
        var existingSeat = SeatGenerator.Create(lan);

        await SetupAggregates(GuildId, existingSeat);

        //Act
        var response = await MakeRequest(client, lan.Id, existingSeat.Id, request);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var seat = GetCommittedDocuments<ProjectedSeat>(GuildId).Should().ContainSingle().Subject;
                Assert.Multiple(
                    () =>
                    {
                        if (request.Title is not null) seat.Title.Should().Be(seat.Title);
                    },
                    () =>
                    {
                        if (request.Bounds is not null) seat.Bounds.Should().BeEquivalentTo(seat.Bounds);
                    });
            });
    }

    [Fact]
    public async Task fails_when_seat_does_not_exists()
    {
        // Arrange
        var client = GetClient(GuildId, Role.Operator);

        var lan = LanGenerator.Create(GuildId);
        await SetupAggregates(GuildId, lan);

        //Act
        var response = await MakeRequest(client, lan.Id, Guid.NewGuid(), Generator.UpdateSeatRequest());

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public static IEnumerable<object[]> InvalidUpdateRequests = new[]
    {
        new object[] { Generator.UpdateSeatRequest() with { Title = "" } },
        new object[]
            { Generator.UpdateSeatRequest() with { Bounds = new Infrastructure.Entrypoints.Http.Bounds(0, 0, -1, 1) } },
        new object[]
            { Generator.UpdateSeatRequest() with { Bounds = new Infrastructure.Entrypoints.Http.Bounds(0, 0, 1, -1) } },
    };

    [Theory]
    [MemberData(nameof(InvalidUpdateRequests))]
    public async Task fails_when_seat_request_model_is_invalid(UpdateEndpoint.Request request)
    {
        // Arrange
        var client = GetClient(GuildId, Role.Operator);

        //Act
        var response = await MakeRequest(client, Guid.NewGuid(), Guid.NewGuid(), request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task fails_when_logged_in_user_has_insufficent_roles()
    {
        // Arrange
        var client = GetClient(GuildId);

        //Act
        var response = await MakeRequest(client, Guid.NewGuid(), Guid.NewGuid(), Generator.UpdateSeatRequest());

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}