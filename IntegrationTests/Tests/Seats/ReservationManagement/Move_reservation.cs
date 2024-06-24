using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Application.Features.Reservation;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.ReservationManagement;

// ReSharper disable once InconsistentNaming
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class Move_reservation : IntegrationTestBase
{
    public Move_reservation(TestWebApplicationFactory factory, PostgresFixture databaseFixture, ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    private static Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId, Guid lanId, Guid fromSeatId, Guid toSeatId) =>
        client.PutAsJsonAsync(
            $"guild/{guildId}/lan/{lanId}/seat/{fromSeatId}/reservationmanagement",
            new MoveReservationFor.Request(toSeatId));

    [Fact]
    public async Task succeeds_when_seat_is_reserved_and_target_seat_is_not_reserved()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Operator);

        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var reservedBy = CreateUser(guild.Id);
        var fromSeat = SeatGenerator.Create(lan, CreateUser(lan.GuildId), reservedBy: reservedBy);
        var toSeat = SeatGenerator.Create(lan, CreateUser(lan.GuildId));

        await SetupAggregates(guild.Id, lan, fromSeat, toSeat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, fromSeat.Id, toSeat.Id);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedFromSeat = GetCommittedDocuments<ProjectedSeat>(guild.Id)
                    .Should()
                    .ContainSingle(seat => seat.Id == fromSeat.Id)
                    .Subject;

                var committedToSeat = GetCommittedDocuments<ProjectedSeat>(guild.Id)
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
		var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Operator);

        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var fromSeat = SeatGenerator.Create(lan, CreateUser(lan.GuildId));
        var toSeat = SeatGenerator.Create(lan, CreateUser(lan.GuildId));

        await SetupAggregates(guild.Id, lan, fromSeat, toSeat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, fromSeat.Id, toSeat.Id);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.NotFound),
            () =>
            {
                var committedFromSeat = GetCommittedDocuments<ProjectedSeat>(guild.Id)
                    .Should()
                    .ContainSingle(seat => seat.Id == fromSeat.Id)
                    .Subject;

                var committedToSeat = GetCommittedDocuments<ProjectedSeat>(guild.Id)
                    .Should()
                    .ContainSingle(seat => seat.Id == toSeat.Id)
                    .Subject;

                committedFromSeat.ReservedBy.Should().Be(fromSeat.ReservedBy);
                committedToSeat.ReservedBy.Should().BeNull();
            });
    }

    [Fact]
    public async Task fails_when_target_seat_is_reserved()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Operator);
        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var fromSeat = SeatGenerator.Create(lan, CreateUser(lan.GuildId), reservedBy: CreateUser(guild.Id));
        var reservedBy = CreateUser(guild.Id);
        var toSeat = SeatGenerator.Create(lan, CreateUser(lan.GuildId), reservedBy: reservedBy);

        await SetupAggregates(guild.Id, lan, fromSeat, toSeat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, fromSeat.Id, toSeat.Id);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Conflict),
            () =>
            {
                var committedFromSeat = GetCommittedDocuments<ProjectedSeat>(guild.Id)
                    .Should()
                    .ContainSingle(seat => seat.Id == fromSeat.Id)
                    .Subject;

                var committedToSeat = GetCommittedDocuments<ProjectedSeat>(guild.Id)
                    .Should()
                    .ContainSingle(seat => seat.Id == toSeat.Id)
                    .Subject;

                committedFromSeat.ReservedBy.Should().Be(fromSeat.ReservedBy);
                committedToSeat.ReservedBy.Should().Be(toSeat.ReservedBy);
            });
    }

    [Fact]
    public async Task fails_when_seat_and_target_seat_is_reserved()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Operator);

        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var reservedByFrom = CreateUser(guild.Id);
        var fromSeat = SeatGenerator.Create(lan, CreateUser(lan.GuildId), reservedBy: reservedByFrom);
        var reservedByTo = CreateUser(guild.Id);
        var toSeat = SeatGenerator.Create(lan, CreateUser(lan.GuildId), reservedBy: reservedByTo);

        await SetupAggregates(guild.Id, lan, fromSeat, toSeat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, fromSeat.Id, toSeat.Id);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Conflict),
            () =>
            {
                var committedFromSeat = GetCommittedDocuments<ProjectedSeat>(guild.Id)
                    .Should()
                    .ContainSingle(seat => seat.Id == fromSeat.Id)
                    .Subject;

                var committedToSeat = GetCommittedDocuments<ProjectedSeat>(guild.Id)
                    .Should()
                    .ContainSingle(seat => seat.Id == toSeat.Id)
                    .Subject;

                committedFromSeat.ReservedBy.Should().Be(fromSeat.ReservedBy);
                committedToSeat.ReservedBy.Should().Be(toSeat.ReservedBy);
            });
    }

    [Fact]
    public async Task fails_when_logged_in_user_has_insufficent_roles()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id);

        // Act
        var response = await MakeRequest(client, guild.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}