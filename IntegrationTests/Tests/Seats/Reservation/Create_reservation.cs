using System.Net;
using FluentAssertions;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Authentication;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.Reservation;

// ReSharper disable once InconsistentNaming
public class Create_reservation : IntegrationTestBase
{
    public Create_reservation(TestWebApplicationFactory factory, PostgresFixture databaseFixture, ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client, Guid lanId, Guid seatId) =>
        await client.PostAsync($"lan/{lanId}/seat/{seatId}/reservation", null);

    [Fact]
    public async Task succeeds_when_reserving_existing_available_seat()
    {
        // Arrange
        var identity = await CreateIdentity(Role.User);
        var client = GetClient(identity);

        var lan = LanGenerator.Create(GuildId);
        var seat = SeatGenerator.Create(lan);

        await SetupAggregates(seat);

        //Act
        var response = await MakeRequest(client, lan.Id, seat.Id);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>().Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(identity.User.Id);
            });
    }

    [Fact]
    public async Task succeeds_when_reserving_seat_that_user_has_already_reserved()
    {
        // Arrange
        var identity = await CreateIdentity(Role.User);
        var client = GetClient(identity);

        var lan = LanGenerator.Create(GuildId);
        var seat = SeatGenerator.Create(lan, reservedBy: identity.User);

        await SetupAggregates(seat);

        //Act
        var response = await MakeRequest(client, lan.Id, seat.Id);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>().Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(identity.User.Id);
            });
    }

    [Fact]
    public async Task fails_when_seat_is_already_reserved()
    {
        // Arrange
        var client = GetClient();

        var alreadyReservedBy = await CreateUser();

        var lan = LanGenerator.Create(GuildId);
        var seat = SeatGenerator.Create(lan, reservedBy: alreadyReservedBy);

        await SetupAggregates(seat);

        //Act
        var response = await MakeRequest(client, lan.Id, seat.Id);

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
        var client = GetClient();

        var lan = LanGenerator.Create(GuildId);
        var seat = SeatGenerator.Create(lan);

        //Act
        var response = await MakeRequest(client, lan.Id, seat.Id);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task fails_when_user_already_has_a_reserved_seat()
    {
        // Arrange
        var identity = await CreateIdentity(Role.User);
        var client = GetClient(identity);

        var lan = LanGenerator.Create(GuildId);
        var alreadyReservedSeat = SeatGenerator.Create(lan, reservedBy: identity.User);
        var seat = SeatGenerator.Create(lan);

        await SetupAggregates(alreadyReservedSeat, seat);

        //Act
        var response = await MakeRequest(client, lan.Id, seat.Id);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>().Should()
                    .ContainSingle(x => x.Id == seat.Id).Subject;
                committedSeat.ReservedBy.Should().BeNull();
            });
    }
}