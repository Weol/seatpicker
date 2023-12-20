using System.Net;
using FluentAssertions;
using Seatpicker.Application.Features.Seats;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.Reservation;

// ReSharper disable once InconsistentNaming
public class Remove_reservation : IntegrationTestBase
{
    public Remove_reservation(TestWebApplicationFactory factory, PostgresFixture databaseFixture, ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client, Guid lanId, Guid seatId) =>
        await client.DeleteAsync($"lan/{lanId}/seat/{seatId}/reservation");

    [Fact]
    public async Task succeeds_when_seat_is_reserved_by_user()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var lan = LanGenerator.Create(GuildId);
        var seat = SeatGenerator.Create(lan, reservedBy: identity.User);

        await SetupAggregates(lan, seat);

        //Act
        var response = await MakeRequest(client, lan.Id, seat.Id);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>().Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().BeNull();
            });
    }

    [Fact]
    public async Task succeeds_when_seat_is_not_reserved()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var lan = LanGenerator.Create(GuildId);
        var seat = SeatGenerator.Create(lan);

        await SetupAggregates(seat);

        //Act
        var response = await MakeRequest(client, lan.Id, seat.Id);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task fails_when_seat_is_reserved_by_different_user()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var alreadyReservedBy = await CreateUser();
        var lan = LanGenerator.Create(GuildId);
        var seat = SeatGenerator.Create(lan, reservedBy: alreadyReservedBy);

        await SetupAggregates(seat);

        //Act
        var response = await MakeRequest(client, lan.Id, seat.Id);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Conflict),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>().Should().ContainSingle().Subject;
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

        //Act
        var response = await client.DeleteAsync($"reservation/{Guid.NewGuid()}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}