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
public class Remove_reservation : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public Remove_reservation(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }

    [Fact]
    public async Task succeeds_when_seat_is_reserved_by_user()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var seat = SeatGenerator.Create(reservedBy: identity.User);

        SetupAggregates(seat);

        //Act
        var response = await client.DeleteAsync($"reservation/{seat.Id}");

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedAggregates<Seat>().Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().BeNull();
            });
    }

    [Fact]
    public async Task succeeds_when_seat_is_not_reserved()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var seat = SeatGenerator.Create();

        SetupAggregates(seat);

        //Act
        var response = await client.DeleteAsync($"reservation/{seat.Id}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task fails_when_seat_is_reserved_by_different_user()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var alreadyReservedBy = UserGenerator.Create();

        var seat = SeatGenerator.Create(reservedBy: alreadyReservedBy);

        SetupAggregates(seat);

        //Act
        var response = await client.DeleteAsync($"reservation/{seat.Id}");

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Conflict),
            () =>
            {
                var committedSeat = GetCommittedAggregates<Seat>().Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Id.Should().Be(alreadyReservedBy.Id);
            });
    }

    [Fact]
    public async Task fails_when_seat_does_not_exist()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var seat = SeatGenerator.Create();

        //Act
        var response = await client.DeleteAsync($"reservation/{seat.Id}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}