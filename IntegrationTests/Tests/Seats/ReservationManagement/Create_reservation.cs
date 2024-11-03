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
public class Create_reservation(
    TestWebApplicationFactory fusery,
    PostgresFixture databaseFixture,
    ITestOutputHelper testOutputHelper) : IntegrationTestBase(fusery, databaseFixture, testOutputHelper)
{
    private static async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId, string lanId, string seatId, string userId) => await client.PostAsJsonAsync(
            $"guild/{guildId}/lan/{lanId}/seat/{seatId}/reservationmanagement",
            new CreateReservationFor.Request(userId));
    
    [Fact]
    public async Task succeeds_when_reserving_existing_available_seat()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Operator);
        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var seat = SeatGenerator.Create(lan, CreateUser(guild.Id));
        var reserveFor = CreateUser(guild.Id);

        await SetupAggregates(guild.Id, lan, seat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, seat.Id, reserveFor.Id);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guild.Id).Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(reserveFor.Id);
            });
    }

    [Fact]
    public async Task succeeds_when_reserving_seat_that_user_has_already_reserved()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Operator);

        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var reserveFor = CreateUser(guild.Id);
        var seat = SeatGenerator.Create(lan, CreateUser(guild.Id), reservedBy: reserveFor);

        await SetupAggregates(guild.Id, lan, seat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, seat.Id, reserveFor.Id);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guild.Id).Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(reserveFor.Id);
            });
    }

    [Fact]
    public async Task fails_when_seat_is_already_reserved()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Operator);

        var alreadyReservedBy = CreateUser(guild.Id);
        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var seat = SeatGenerator.Create(lan, CreateUser(guild.Id), reservedBy: alreadyReservedBy);
        var reserveFor = CreateUser(guild.Id);

        await SetupAggregates(guild.Id, lan, seat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, seat.Id, reserveFor.Id);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Conflict),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guild.Id).Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(alreadyReservedBy.Id);
            });
    }

    [Fact]
    public async Task fails_when_seat_does_not_exist()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Operator);

        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var seat = SeatGenerator.Create(lan, CreateUser(guild.Id));
        var reserveFor = CreateUser(guild.Id);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, seat.Id, reserveFor.Id);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task fails_when_user_already_has_a_reserved_seat()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Operator);

        var reserveFor = CreateUser(guild.Id);
        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var alreadyReservedSeat = SeatGenerator.Create(lan, CreateUser(guild.Id), reservedBy: reserveFor);
        var seat = SeatGenerator.Create(lan, CreateUser(guild.Id));

        await SetupAggregates(guild.Id, lan, alreadyReservedSeat, seat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, seat.Id, reserveFor.Id);
        
        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guild.Id).Should().ContainSingle(x => x.Id == seat.Id).Subject;
                committedSeat.ReservedBy.Should().BeNull();
            });
    }
    
    [Fact]
    public async Task fails_when_logged_in_user_has_insufficent_roles()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id);

        // Act
        var response = await MakeRequest(client, guild.Id, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}