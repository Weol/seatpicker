using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.ReservationManagement;

// ReSharper disable once InconsistentNaming
public class Move_reservation : IntegrationTestBase
{
    public Move_reservation(TestWebApplicationFactory factory, PostgresFixture databaseFixture, ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    private Task<HttpResponseMessage> MakeRequest(HttpClient client, Guid lanId, Guid fromSeatId, Guid toSeatId) =>
        client.PutAsJsonAsync(
            $"lan/{lanId}/seat/{fromSeatId}/reservationmanagement",
            new MoveEndpoint.Request(toSeatId));

    [Fact]
    public async Task succeeds_when_seat_is_reserved_and_target_seat_is_not_reserved()
    {
        // Arrange
        var client = GetClient(GuildId, Role.Operator);

        var lan = LanGenerator.Create(GuildId);
        var reservedBy = await CreateUser(GuildId);
        var fromSeat = SeatGenerator.Create(lan, reservedBy: reservedBy);
        var toSeat = SeatGenerator.Create(lan);

        await SetupAggregates(GuildId, lan, fromSeat, toSeat);

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
                committedToSeat.ReservedBy!.Should().Be(reservedBy.Id);
            });
    }

    [Fact]
    public async Task fails_when_seat_is_not_reserved()
    {
        // Arrange
        var client = GetClient(GuildId, Role.Operator);

        var lan = LanGenerator.Create(GuildId);
        var fromSeat = SeatGenerator.Create(lan);
        var toSeat = SeatGenerator.Create(lan);

        await SetupAggregates(GuildId, lan, fromSeat, toSeat);

        //Act
        var response = await MakeRequest(client, lan.Id, fromSeat.Id, toSeat.Id);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.NotFound),
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

    [Fact]
    public async Task fails_when_target_seat_is_reserved()
    {
        // Arrange
        var client = GetClient(GuildId, Role.Operator);
        var lan = LanGenerator.Create(GuildId);
        var fromSeat = SeatGenerator.Create(lan, reservedBy: await CreateUser(GuildId));
        var reservedBy = await CreateUser(GuildId);
        var toSeat = SeatGenerator.Create(lan, reservedBy: reservedBy);

        await SetupAggregates(GuildId, lan, fromSeat, toSeat);

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
                committedToSeat.ReservedBy.Should().Be(toSeat.ReservedBy);
            });
    }

    [Fact]
    public async Task fails_when_seat_and_target_seat_is_reserved()
    {
        // Arrange
        var client = GetClient(GuildId, Role.Operator);

        var lan = LanGenerator.Create(GuildId);
        var reservedByFrom = await CreateUser(GuildId);
        var fromSeat = SeatGenerator.Create(lan, reservedBy: reservedByFrom);
        var reservedByTo = await CreateUser(GuildId);
        var toSeat = SeatGenerator.Create(lan, reservedBy: reservedByTo);

        await SetupAggregates(GuildId, lan, fromSeat, toSeat);

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
                committedToSeat.ReservedBy.Should().Be(toSeat.ReservedBy);
            });
    }

    [Fact]
    public async Task fails_when_logged_in_user_has_insufficent_roles()
    {
        // Arrange
        var client = GetClient(GuildId);

        //Act
        var response = await MakeRequest(client, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}