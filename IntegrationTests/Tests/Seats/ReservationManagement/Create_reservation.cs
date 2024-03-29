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
public class Create_reservation : IntegrationTestBase
{
    public Create_reservation(TestWebApplicationFactory factory, PostgresFixture databaseFixture, ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client, Guid lanId, Guid seatId, string userId) => await client.PostAsJsonAsync(
            $"lan/{lanId}/seat/{seatId}/reservationmanagement",
            new CreateEndpoint.Request(userId));
    
    [Fact]
    public async Task succeeds_when_reserving_existing_available_seat()
    {
        // Arrange
        var client = GetClient(Role.Operator);

        var lan = LanGenerator.Create(GuildId);
        var seat = SeatGenerator.Create(lan);
        var reserveFor = await CreateUser();

        await SetupAggregates(lan, seat);

        //Act
        var response = await MakeRequest(client, lan.Id, seat.Id, reserveFor.Id);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>().Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(reserveFor.Id);
            });
    }

    [Fact]
    public async Task succeeds_when_reserving_seat_that_user_has_already_reserved()
    {
        // Arrange
        var client = GetClient(Role.Operator);

        var lan = LanGenerator.Create(GuildId);
        var reserveFor = await CreateUser();
        var seat = SeatGenerator.Create(lan, reservedBy: reserveFor);

        await SetupAggregates(lan, seat);

        //Act
        var response = await MakeRequest(client, lan.Id, seat.Id, reserveFor.Id);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>().Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(reserveFor.Id);
            });
    }

    [Fact]
    public async Task fails_when_seat_is_already_reserved()
    {
        // Arrange
        var client = GetClient(Role.Operator);

        var alreadyReservedBy = await CreateUser();
        var lan = LanGenerator.Create(GuildId);
        var seat = SeatGenerator.Create(lan, reservedBy: alreadyReservedBy);
        var reserveFor = await CreateUser();

        await SetupAggregates(lan, seat);

        //Act
        var response = await MakeRequest(client, lan.Id, seat.Id, reserveFor.Id);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Conflict),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>().Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(alreadyReservedBy.Id);
            });
    }

    [Fact]
    public async Task fails_when_seat_does_not_exist()
    {
        // Arrange
        var client = GetClient(Role.Operator);

        var lan = LanGenerator.Create(GuildId);
        var seat = SeatGenerator.Create(lan);
        var reserveFor = await CreateUser();

        //Act
        var response = await MakeRequest(client, lan.Id, seat.Id, reserveFor.Id);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task fails_when_user_already_has_a_reserved_seat()
    {
        // Arrange
        var client = GetClient(Role.Operator);

        var reserveFor = await CreateUser();
        var lan = LanGenerator.Create(GuildId);
        var alreadyReservedSeat = SeatGenerator.Create(lan, reservedBy: reserveFor);
        var seat = SeatGenerator.Create(lan);

        await SetupAggregates(lan, alreadyReservedSeat, seat);

        //Act
        var response = await MakeRequest(client, lan.Id, seat.Id, reserveFor.Id);
        
        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>().Should().ContainSingle(x => x.Id == seat.Id).Subject;
                committedSeat.ReservedBy.Should().BeNull();
            });
    }
    
    [Fact]
    public async Task fails_when_logged_in_user_has_insufficent_roles()
    {
        // Arrange
        var client = GetClient();

        //Act
        var response = await MakeRequest(client, Guid.NewGuid(), Guid.NewGuid(), "123");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}