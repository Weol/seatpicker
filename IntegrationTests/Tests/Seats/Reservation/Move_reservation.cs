using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Application.Features.Reservation;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Reservation;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.Reservation;

// ReSharper disable once InconsistentNaming
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class Move_reservation(
    TestWebApplicationFactory factory,
    PostgresFixture databaseFixture,
    ITestOutputHelper testOutputHelper) : IntegrationTestBase(factory, databaseFixture, testOutputHelper)
{
    private static async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId, string lanId, string seatId, string toSeatId) =>
        await client.PutAsJsonAsync(
            $"guild/{guildId}/lan/{lanId}/seat/{seatId}/reservation",
            new MoveReservation.Request(toSeatId));

    [Fact]
    public async Task succeeds_when_seat_is_reserved_by_user_and_other_seat_is_not_reserved()
    {
        // Arrange
		var guild = await CreateGuild();
        var identity = await CreateIdentity(guild.Id, Role.User);
        var client = GetClient(identity);

        var lan = RandomData.Aggregates.Lan(CreateUser(guild.Id));
        var fromSeat = SeatGenerator.Create(lan, CreateUser(guild.Id), reservedBy: identity.User);
        var toSeat = SeatGenerator.Create(lan, CreateUser(guild.Id));

        await SetupAggregates(guild.Id, fromSeat, toSeat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, fromSeat.Id, toSeat.Id);

        // Assert
        var committedSeats = await GetCommittedDocuments<ProjectedSeat>(guild.Id);
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedFromSeat = committedSeats 
                    .Should()
                    .ContainSingle(seat => seat.Id == fromSeat.Id)
                    .Subject;

                var committedToSeat = committedSeats
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
		var guild = await CreateGuild();
        var client = GetClient(guild.Id);

        var alreadyReservedBy = CreateUser(guild.Id);
        var lan = RandomData.Aggregates.Lan(CreateUser(guild.Id));
        var fromSeat = SeatGenerator.Create(lan, CreateUser(guild.Id), reservedBy: alreadyReservedBy);
        var toSeat = SeatGenerator.Create(lan, CreateUser(guild.Id));

        await SetupAggregates(guild.Id, fromSeat, toSeat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, fromSeat.Id, toSeat.Id);

        // Assert
        var committedSeats = await GetCommittedDocuments<ProjectedSeat>(guild.Id);
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Conflict),
            () =>
            {
                var committedFromSeat = committedSeats
                    .Should()
                    .ContainSingle(seat => seat.Id == fromSeat.Id)
                    .Subject;

                var committedToSeat = committedSeats
                    .Should()
                    .ContainSingle(seat => seat.Id == toSeat.Id)
                    .Subject;

                committedFromSeat.ReservedBy.Should().Be(fromSeat.ReservedBy);
                committedToSeat.ReservedBy.Should().BeNull();
            });
    }
}