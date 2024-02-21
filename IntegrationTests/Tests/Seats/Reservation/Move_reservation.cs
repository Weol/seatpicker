using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints.Http.Reservation;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.Reservation;

// ReSharper disable once InconsistentNaming
public class Move_reservation : IntegrationTestBase
{
    public Move_reservation(TestWebApplicationFactory factory, PostgresFixture databaseFixture, ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client, Guid lanId, Guid seatId, Guid toSeatId) =>
        await client.PutAsJsonAsync(
            $"lan/{lanId}/seat/{seatId}/reservation",
            new MoveReservation.Request(toSeatId));

    [Fact]
    public async Task succeeds_when_seat_is_reserved_by_user_and_other_seat_is_not_reserved()
    {
        // Arrange
        var identity = await CreateIdentity(GuildId, Role.User);
        var client = GetClient(GuildId, identity);

        var lan = LanGenerator.Create(GuildId);
        var fromSeat = SeatGenerator.Create(lan, reservedBy: identity.User);
        var toSeat = SeatGenerator.Create(lan);

        await SetupAggregates(GuildId, fromSeat, toSeat);

        //Act
        var response = await MakeRequest(client, lan.Id, fromSeat.Id, toSeat.Id);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedFromSeat = GetCommittedDocuments<ProjectedSeat>(GuildId)
                    .Should()
                    .ContainSingle(seat => seat.Id == fromSeat.Id)
                    .Subject;

                var committedToSeat = GetCommittedDocuments<ProjectedSeat>(GuildId)
                    .Should()
                    .ContainSingle(seat => seat.Id == toSeat.Id)
                    .Subject;

                committedFromSeat.ReservedBy.Should().BeNull();
                committedToSeat.ReservedBy.Should().NotBeNull();

                committedToSeat.ReservedBy!.Should().Be(identity.User.Id);
            });
    }

    [Fact]
    public async Task fails_when_seat_is_reserved_by_another_user()
    {
        // Arrange
        var client = GetClient(GuildId);

        var alreadyReservedBy = await CreateUser(GuildId);
        var lan = LanGenerator.Create(GuildId);
        var fromSeat = SeatGenerator.Create(lan, reservedBy: alreadyReservedBy);
        var toSeat = SeatGenerator.Create(lan);

        await SetupAggregates(GuildId, fromSeat, toSeat);

        //Act
        var response = await MakeRequest(client, lan.Id, fromSeat.Id, toSeat.Id);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Conflict),
            () =>
            {
                var committedFromSeat = GetCommittedDocuments<ProjectedSeat>(GuildId)
                    .Should()
                    .ContainSingle(seat => seat.Id == fromSeat.Id)
                    .Subject;

                var committedToSeat = GetCommittedDocuments<ProjectedSeat>(GuildId)
                    .Should()
                    .ContainSingle(seat => seat.Id == toSeat.Id)
                    .Subject;

                committedFromSeat.ReservedBy.Should().Be(fromSeat.ReservedBy);
                committedToSeat.ReservedBy.Should().BeNull();
            });
    }
}