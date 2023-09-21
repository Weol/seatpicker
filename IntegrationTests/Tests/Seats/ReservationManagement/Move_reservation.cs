using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Reservation;
using Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.ReservationManagement;

// ReSharper disable once InconsistentNaming
public class Move_reservation : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public Move_reservation(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }

    [Fact]
    public async Task succeeds_when_seat_is_reserved_and_other_seat_is_not_reserved()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var reservedBy = CreateUser();
        var fromSeat = SeatGenerator.Create(reservedBy: reservedBy);
        var toSeat = SeatGenerator.Create();

        SetupAggregates(fromSeat, toSeat);

        //Act
        var response = await client.PutAsJsonAsync(
            $"reservationmanagement/{fromSeat.Id}",
            new ReservationManagementController.MoveReservationForRequestModel(reservedBy.Id, fromSeat.Id, toSeat.Id));

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedFromSeat = GetCommittedAggregates<Seat>()
                    .Should()
                    .ContainSingle(seat => seat.Id == fromSeat.Id)
                    .Subject;

                var committedToSeat = GetCommittedAggregates<Seat>()
                    .Should()
                    .ContainSingle(seat => seat.Id == toSeat.Id)
                    .Subject;

                committedFromSeat.ReservedBy.Should().BeNull();

                committedToSeat.ReservedBy.Should().NotBeNull();
                committedToSeat.ReservedBy!.Should().Be(reservedBy.Id);
            });
    }

    [Fact]
    public async Task fails_when_seat_is_not_reserved()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var fromSeat = SeatGenerator.Create();
        var toSeat = SeatGenerator.Create();

        SetupAggregates(fromSeat, toSeat);

        //Act
        var response = await client.PutAsJsonAsync(
            $"reservationmanagement/{fromSeat.Id}",
            new ReservationManagementController.MoveReservationForRequestModel(CreateUser().Id, fromSeat.Id, toSeat.Id));

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.NotFound),
            () =>
            {
                var committedFromSeat = GetCommittedAggregates<Seat>()
                    .Should()
                    .ContainSingle(seat => seat.Id == fromSeat.Id)
                    .Subject;

                var committedToSeat = GetCommittedAggregates<Seat>()
                    .Should()
                    .ContainSingle(seat => seat.Id == toSeat.Id)
                    .Subject;

                committedFromSeat.ReservedBy.Should().Be(fromSeat.ReservedBy);
                committedToSeat.ReservedBy.Should().BeNull();
            });
    }

    [Fact]
    public async Task fails_when_seat_is_not_reserved_by_user()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var fromSeat = SeatGenerator.Create(reservedBy: CreateUser());
        var toSeat = SeatGenerator.Create();

        SetupAggregates(fromSeat, toSeat);

        //Act
        var response = await client.PutAsJsonAsync(
            $"reservationmanagement/{fromSeat.Id}",
            new ReservationManagementController.MoveReservationForRequestModel(CreateUser().Id, fromSeat.Id, toSeat.Id));

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Conflict),
            () =>
            {
                var committedFromSeat = GetCommittedAggregates<Seat>()
                    .Should()
                    .ContainSingle(seat => seat.Id == fromSeat.Id)
                    .Subject;

                var committedToSeat = GetCommittedAggregates<Seat>()
                    .Should()
                    .ContainSingle(seat => seat.Id == toSeat.Id)
                    .Subject;

                committedFromSeat.ReservedBy.Should().Be(fromSeat.ReservedBy);
                committedToSeat.ReservedBy.Should().BeNull();
            });
    }
}