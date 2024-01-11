using System.Net;
using FluentAssertions;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Authentication;
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

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client, Guid lanId, Guid seatId) 
        => await client.DeleteAsync($"lan/{lanId}/seat/{seatId}/reservationmanagement");
    
    [Fact]
    public async Task succeeds_when_seat_is_reserved_by_logged_in_user()
    {
        // Arrange
        var identity = await CreateIdentity(GuildId, Role.Operator);
        var client = GetClient(GuildId, identity);

        var lan = LanGenerator.Create(GuildId);
        var seat = SeatGenerator.Create(lan, reservedBy: identity.User);

        await SetupAggregates(GuildId, lan, seat);

        //Act
        var response = await MakeRequest(client, lan.Id, seat.Id);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(GuildId).Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().BeNull();
            });
    }

    [Fact]
    public async Task succeeds_when_seat_is_reserved_by_a_different_user()
    {
        // Arrange
        var client = GetClient(GuildId, Role.Operator);

        var user = await CreateUser(GuildId);
        var lan = LanGenerator.Create(GuildId);
        var seat = SeatGenerator.Create(lan, reservedBy: user);

        await SetupAggregates(GuildId, lan, seat);

        //Act
        var response = await MakeRequest(client, lan.Id, seat.Id);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(GuildId).Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().BeNull();
            });
    }
    
    [Fact]
    public async Task succeeds_when_seat_is_not_reserved()
    {
        // Arrange
        var client = GetClient(GuildId, Role.Operator);

        var user = await CreateUser(GuildId);
        var lan = LanGenerator.Create(GuildId);
        var seat = SeatGenerator.Create(lan, reservedBy: user);

        await SetupAggregates(GuildId, seat);

        //Act
        var response = await MakeRequest(client, lan.Id, seat.Id);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task fails_when_logged_in_user_has_insufficent_roles()
    {
        // Arrange
        var client = GetClient(GuildId);

        //Act
        var response = await MakeRequest(client, Guid.NewGuid(), Guid.NewGuid());

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    
    [Fact]
    public async Task fails_when_seat_does_not_exist()
    {
        // Arrange
        var client = GetClient(GuildId, Role.Operator);

        //Act
        var response = await MakeRequest(client, Guid.NewGuid(), Guid.NewGuid());

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}