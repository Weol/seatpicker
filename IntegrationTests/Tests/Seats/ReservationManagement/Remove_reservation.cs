using System.Net;
using FluentAssertions;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Domain;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.ReservationManagement;

// ReSharper disable once InconsistentNaming
public class Remove_reservation : IntegrationTestBase
{
    public Remove_reservation(TestWebApplicationFactory factory, PostgresFixture databaseFixture, ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId, Guid lanId, Guid seatId) 
        => await client.DeleteAsync($"guild/{guildId}/lan/{lanId}/seat/{seatId}/reservationmanagement");
    
    [Fact]
    public async Task succeeds_when_seat_is_reserved_by_logged_in_user()
    {
        // Arrange
        var guildId = await CreateGuild();
        var identity = await CreateIdentity(guildId, Role.Operator);
        var client = GetClient(identity);

        var lan = LanGenerator.Create(guildId, CreateUser(guildId));
        var seat = SeatGenerator.Create(lan, CreateUser(lan.GuildId), reservedBy: identity.User);

        await SetupAggregates(guildId, lan, seat);

        // Act
        var response = await MakeRequest(client, guildId, lan.Id, seat.Id);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guildId).Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().BeNull();
            });
    }

    [Fact]
    public async Task succeeds_when_seat_is_reserved_by_a_different_user()
    {
        // Arrange
		var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Operator);

        var user = CreateUser(guildId);
        var lan = LanGenerator.Create(guildId, CreateUser(guildId));
        var seat = SeatGenerator.Create(lan, CreateUser(lan.GuildId), reservedBy: user);

        await SetupAggregates(guildId, lan, seat);

        // Act
        var response = await MakeRequest(client, guildId, lan.Id, seat.Id);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guildId).Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().BeNull();
            });
    }
    
    [Fact]
    public async Task succeeds_when_seat_is_not_reserved()
    {
        // Arrange
		var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Operator);

        var user = CreateUser(guildId);
        var lan = LanGenerator.Create(guildId, CreateUser(guildId));
        var seat = SeatGenerator.Create(lan, CreateUser(lan.GuildId), reservedBy: user);

        await SetupAggregates(guildId, seat);

        // Act
        var response = await MakeRequest(client, guildId, lan.Id, seat.Id);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task fails_when_logged_in_user_has_insufficent_roles()
    {
        // Arrange
		var guildId = await CreateGuild();
        var client = GetClient(guildId);

        // Act
        var response = await MakeRequest(client, guildId, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    
    [Fact]
    public async Task fails_when_seat_does_not_exist()
    {
        // Arrange
		var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Operator);

        // Act
        var response = await MakeRequest(client, guildId, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}