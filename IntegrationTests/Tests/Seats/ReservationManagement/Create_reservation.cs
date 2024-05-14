using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.ReservationManagement;

// ReSharper disable once InconsistentNaming
public class Create_reservation : IntegrationTestBase
{
    public Create_reservation(TestWebApplicationFactory factory, PostgresFixture databaseFixture, ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId, Guid lanId, Guid seatId, string userId) => await client.PostAsJsonAsync(
            $"guild/{guildId}/lan/{lanId}/seat/{seatId}/reservationmanagement",
            new CreateReservationFor.Request(userId));
    
    [Fact]
    public async Task succeeds_when_reserving_existing_available_seat()
    {
        // Arrange
		var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Operator);
        var lan = LanGenerator.Create(guildId, CreateUser(guildId));
        var seat = SeatGenerator.Create(lan, CreateUser(lan.GuildId));
        var reserveFor = CreateUser(guildId);

        await SetupAggregates(guildId, lan, seat);

        // Act
        var response = await MakeRequest(client, guildId, lan.Id, seat.Id, reserveFor.Id);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guildId).Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(reserveFor.Id);
            });
    }

    [Fact]
    public async Task succeeds_when_reserving_seat_that_user_has_already_reserved()
    {
        // Arrange
		var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Operator);

        var lan = LanGenerator.Create(guildId, CreateUser(guildId));
        var reserveFor = CreateUser(guildId);
        var seat = SeatGenerator.Create(lan, CreateUser(lan.GuildId), reservedBy: reserveFor);

        await SetupAggregates(guildId, lan, seat);

        // Act
        var response = await MakeRequest(client, guildId, lan.Id, seat.Id, reserveFor.Id);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guildId).Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(reserveFor.Id);
            });
    }

    [Fact]
    public async Task fails_when_seat_is_already_reserved()
    {
        // Arrange
		var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Operator);

        var alreadyReservedBy = CreateUser(guildId);
        var lan = LanGenerator.Create(guildId, CreateUser(guildId));
        var seat = SeatGenerator.Create(lan, CreateUser(lan.GuildId), reservedBy: alreadyReservedBy);
        var reserveFor = CreateUser(guildId);

        await SetupAggregates(guildId, lan, seat);

        // Act
        var response = await MakeRequest(client, guildId, lan.Id, seat.Id, reserveFor.Id);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Conflict),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guildId).Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(alreadyReservedBy.Id);
            });
    }

    [Fact]
    public async Task fails_when_seat_does_not_exist()
    {
        // Arrange
		var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Operator);

        var lan = LanGenerator.Create(guildId, CreateUser(guildId));
        var seat = SeatGenerator.Create(lan, CreateUser(lan.GuildId));
        var reserveFor = CreateUser(guildId);

        // Act
        var response = await MakeRequest(client, guildId, lan.Id, seat.Id, reserveFor.Id);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task fails_when_user_already_has_a_reserved_seat()
    {
        // Arrange
		var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Operator);

        var reserveFor = CreateUser(guildId);
        var lan = LanGenerator.Create(guildId, CreateUser(guildId));
        var alreadyReservedSeat = SeatGenerator.Create(lan, CreateUser(lan.GuildId), reservedBy: reserveFor);
        var seat = SeatGenerator.Create(lan, CreateUser(lan.GuildId));

        await SetupAggregates(guildId, lan, alreadyReservedSeat, seat);

        // Act
        var response = await MakeRequest(client, guildId, lan.Id, seat.Id, reserveFor.Id);
        
        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guildId).Should().ContainSingle(x => x.Id == seat.Id).Subject;
                committedSeat.ReservedBy.Should().BeNull();
            });
    }
    
    [Fact]
    public async Task fails_when_logged_in_user_has_insufficent_roles()
    {
        // Arrange
		var guildId = await CreateGuild();
        var client = GetClient(guildId);

        // Act
        var response = await MakeRequest(client, guildId, Guid.NewGuid(), Guid.NewGuid(), "123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}