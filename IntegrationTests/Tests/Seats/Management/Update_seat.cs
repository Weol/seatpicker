using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints.Http.Seat;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.Management;

// ReSharper disable once InconsistentNaming
public class Update_seat : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public Update_seat(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }

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
        var identity = await CreateIdentity(Role.Operator);
        var client = GetClient(identity);

        var existingSeat = SeatGenerator.Create();
        SetupAggregates(existingSeat);

        //Act
        var response = await client.PutAsJsonAsync($"seat/{existingSeat.Id}", request with { SeatId = existingSeat.Id });

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var seat = GetCommittedAggregates<Seat>().Should().ContainSingle().Subject;
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
        var identity = await CreateIdentity(Role.Operator);
        var client = GetClient(identity);

        var model = Generator.UpdateSeatRequest();

        //Act
        var response = await client.PutAsJsonAsync(
            $"seat/{model.SeatId}",
            model);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public static IEnumerable<object[]> InvalidUpdateRequests = new[]
    {
        new object[] { Generator.CreateSeatRequest() with { Title = "" } },
        new object[]
            { Generator.CreateSeatRequest() with { Bounds = new Infrastructure.Entrypoints.Http.Bounds(0, 0, -1, 1) } },
        new object[]
            { Generator.CreateSeatRequest() with { Bounds = new Infrastructure.Entrypoints.Http.Bounds(0, 0, 1, -1) } },
    };

    [Theory]
    [MemberData(nameof(InvalidUpdateRequests))]
    public async Task fails_when_seat_request_model_is_invalid(CreateEndpoint.Request request)
    {
        // Arrange
        var identity = await CreateIdentity(Role.Operator);
        var client = GetClient(identity);

        //Act
        var response = await client.PostAsJsonAsync("seat", request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}