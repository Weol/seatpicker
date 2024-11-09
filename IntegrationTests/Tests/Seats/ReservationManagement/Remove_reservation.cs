using System.Diagnostics.CodeAnalysis;
using System.Net;
using FluentAssertions;
using Seatpicker.Application.Features.Reservation;
using Seatpicker.Domain;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.ReservationManagement;

// ReSharper disable once InconsistentNaming
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class Remove_reservation(
    TestWebApplicationFactory factory,
    PostgresFixture databaseFixture,
    ITestOutputHelper testOutputHelper) : IntegrationTestBase(factory, databaseFixture, testOutputHelper)
{
    private static async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId, string lanId, string seatId)
        => await client.DeleteAsync($"guild/{guildId}/lan/{lanId}/seat/{seatId}/reservationmanagement");

    [Fact]
    public async Task succeeds_when_seat_is_reserved_by_logged_in_user()
    {
        // Arrange
        var guild = await CreateGuild();
        var identity = await CreateIdentity(guild.Id, Role.Operator);
        var client = GetClient(identity);

        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var seat = SeatGenerator.Create(lan, CreateUser(guild.Id), reservedBy: identity.User);

        await SetupAggregates(guild.Id, lan, seat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, seat.Id);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guild.Id).Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().BeNull();
            });
    }

    [Fact]
    public async Task succeeds_when_seat_is_reserved_by_a_different_user()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Operator);

        var user = CreateUser(guild.Id);
        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var seat = SeatGenerator.Create(lan, CreateUser(guild.Id), reservedBy: user);

        await SetupAggregates(guild.Id, lan, seat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, seat.Id);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guild.Id).Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().BeNull();
            });
    }

    [Fact]
    public async Task succeeds_when_seat_is_not_reserved()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Operator);

        var user = CreateUser(guild.Id);
        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var seat = SeatGenerator.Create(lan, CreateUser(guild.Id), reservedBy: user);

        await SetupAggregates(guild.Id, seat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, seat.Id);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task fails_when_logged_in_user_has_insufficent_roles()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id);

        // Act
        var response = await MakeRequest(client, guild.Id, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task fails_when_seat_does_not_exist()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Operator);

        // Act
        var response = await MakeRequest(client, guild.Id, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}