using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Domain;
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

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId, Guid lanId, Guid seatId, Guid toSeatId) =>
        await client.PutAsJsonAsync(
            $"guild/{guildId}/lan/{lanId}/seat/{seatId}/reservation",
            new MoveReservation.Request(toSeatId));

    [Fact]
    public async Task succeeds_when_seat_is_reserved_by_user_and_other_seat_is_not_reserved()
    {
        // Arrange
		var guildId = await CreateGuild();
        var identity = await CreateIdentity(guildId, Role.User);
        var client = GetClient(identity);

        var lan = LanGenerator.Create(guildId, CreateUser(guildId));
        var fromSeat = SeatGenerator.Create(lan, CreateUser(lan.GuildId), reservedBy: identity.User);
        var toSeat = SeatGenerator.Create(lan, CreateUser(lan.GuildId));

        await SetupAggregates(guildId, fromSeat, toSeat);

        //Act
        var response = await MakeRequest(client, guildId, lan.Id, fromSeat.Id, toSeat.Id);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedFromSeat = GetCommittedDocuments<ProjectedSeat>(guildId)
                    .Should()
                    .ContainSingle(seat => seat.Id == fromSeat.Id)
                    .Subject;

                var committedToSeat = GetCommittedDocuments<ProjectedSeat>(guildId)
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
		var guildId = await CreateGuild();
        var client = GetClient(guildId);

        var alreadyReservedBy = CreateUser(guildId);
        var lan = LanGenerator.Create(guildId, CreateUser(guildId));
        var fromSeat = SeatGenerator.Create(lan, CreateUser(lan.GuildId), reservedBy: alreadyReservedBy);
        var toSeat = SeatGenerator.Create(lan, CreateUser(lan.GuildId));

        await SetupAggregates(guildId, fromSeat, toSeat);

        //Act
        var response = await MakeRequest(client, guildId, lan.Id, fromSeat.Id, toSeat.Id);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Conflict),
            () =>
            {
                var committedFromSeat = GetCommittedDocuments<ProjectedSeat>(guildId)
                    .Should()
                    .ContainSingle(seat => seat.Id == fromSeat.Id)
                    .Subject;

                var committedToSeat = GetCommittedDocuments<ProjectedSeat>(guildId)
                    .Should()
                    .ContainSingle(seat => seat.Id == toSeat.Id)
                    .Subject;

                committedFromSeat.ReservedBy.Should().Be(fromSeat.ReservedBy);
                committedToSeat.ReservedBy.Should().BeNull();
            });
    }
}