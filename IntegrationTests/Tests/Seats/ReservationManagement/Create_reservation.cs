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
public class Create_reservation : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public Create_reservation(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }

    [Fact]
    public async Task succeeds_when_reserving_existing_available_seat()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var seat = SeatGenerator.Create();
        var reserveFor = UserGenerator.Create();

        SetupAggregates(seat);

        //Act
        var response = await client.PostAsync(
            "reservationmanagement",
            JsonContent.Create(new ReservationManagementController.CreateReservationForRequestModel(seat.Id, reserveFor.Id)));

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedAggregates<Seat>().Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(reserveFor.Id);
            });
    }

    [Fact]
    public async Task succeeds_when_reserving_seat_that_user_has_already_reserved()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var seat = SeatGenerator.Create(reservedBy: identity.User);
        var reserveFor = UserGenerator.Create();

        SetupAggregates(seat);

        //Act
        var response = await client.PostAsJsonAsync(
            "reservationmanagement",
            new ReservationManagementController.CreateReservationForRequestModel(seat.Id, reserveFor.Id));

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedAggregates<Seat>().Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(reserveFor.Id);
            });
    }

    [Fact]
    public async Task fails_when_seat_is_already_reserved()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var alreadyReservedBy = UserGenerator.Create();
        var seat = SeatGenerator.Create(reservedBy: alreadyReservedBy);
        var reserveFor = UserGenerator.Create();

        SetupAggregates(seat);

        //Act
        var response = await client.PostAsJsonAsync(
            "reservationmanagement",
            new ReservationManagementController.CreateReservationForRequestModel(seat.Id, reserveFor.Id));

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Conflict),
            () =>
            {
                var committedSeat = GetCommittedAggregates<Seat>().Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(alreadyReservedBy.Id);
            });
    }

    [Fact]
    public async Task fails_when_seat_does_not_exist()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var seat = SeatGenerator.Create();
        var reserveFor = UserGenerator.Create();

        //Act
        var response = await client.PostAsJsonAsync(
            "reservationmanagement",
            new ReservationManagementController.CreateReservationForRequestModel(seat.Id, reserveFor.Id));

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}