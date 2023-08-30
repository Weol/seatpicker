using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Reservation;
using Seatpicker.IntegrationTests.Tests.LanManagement;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Reservation;

// ReSharper disable once InconsistentNaming
public class Move_reservation : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public Move_reservation(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }

    [Fact]
    public async Task succeeds_when_seat_is_reserved_by_user_and_other_seat_is_not_reserved()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var fromSeat = SeatGenerator.Create(reservedBy: identity.User);
        var toSeat = SeatGenerator.Create();

        SetupAggregates(fromSeat, toSeat);

        //Act
        var response = await client.PutAsync(
            $"reservation/{fromSeat.Id}",
            JsonContent.Create(new ReservationController.MoveReservationRequestModel(fromSeat.Id, toSeat.Id)));

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

                committedToSeat.ReservedBy!.Id.Should().Be(identity.User.Id);
            });
    }
}