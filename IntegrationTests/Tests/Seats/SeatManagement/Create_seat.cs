using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Reservation;
using Seatpicker.Infrastructure.Entrypoints.Http.Seat;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Reservation;

// ReSharper disable once InconsistentNaming
public class Create_seat : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public Create_seat(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }
    [Fact]
    public async Task succeeds_when_creating_new_seat()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Operator);
        var client = GetClient(identity);

        var seat = SeatGenerator.Create(initiator: identity.User);

        //Act
        var response = await client.PostAsync(
                    "seat",
                    JsonContent.Create(
                        new SeatController.CreateSeatRequestModel(
                            seat.Id,
                            seat.Title,
                            new SeatController.BoundsModel(
                                seat.Bounds.X,
                                seat.Bounds.Y,
                                seat.Bounds.Width,
                                seat.Bounds.Height))));

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedAggregates<Seat>().Should().ContainSingle().Subject;
                committedSeat.Should().BeEquivalentTo(seat);
            });
    }

    [Fact]
    public async Task fails_when_seat_with_same_id_already_exists()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Operator);
        var client = GetClient(identity);

        var existingSeat = SeatGenerator.Create();

        SetupAggregates(existingSeat);

        var seat = SeatGenerator.Create(id: existingSeat.Id, title: "another title", initiator: identity.User);

        //Act
        var response = await client.PostAsync(
            "seat",
            JsonContent.Create(
                new SeatController.CreateSeatRequestModel(
                    seat.Id,
                    seat.Title,
                    new SeatController.BoundsModel(
                        seat.Bounds.X,
                        seat.Bounds.Y,
                        seat.Bounds.Width,
                        seat.Bounds.Height))));

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Conflict),
            () =>
            {
                var committedSeat = GetCommittedAggregates<Seat>().Should().ContainSingle().Subject;
                committedSeat.Should().NotBeEquivalentTo(seat);
            });
    }

    [Theory]
    [MemberData()]
    public async Task fails_when_seat_request_model_is_invalid(SeatController.CreateSeatRequestModel model)
    {
        // Arrange
        var identity = await CreateIdentity(Role.Operator);
        var client = GetClient(identity);

        var seat = SeatGenerator.Create();

        //Act
        var response = await client.PostAsync(
            "seat",
            JsonContent.Create(
                new SeatController.CreateSeatRequestModel(
                    seat.Id,
                    "", // Must set title here otherwise Seat constructor throws exception
                    new SeatController.BoundsModel(
                        seat.Bounds.X,
                        seat.Bounds.Y,
                        seat.Bounds.Width,
                        seat.Bounds.Height))));

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}