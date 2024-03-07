using System.Net;
using FluentAssertions;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Domain;
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

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId, Guid lanId, Guid seatId) =>
        await client.PostAsync($"guild/{guildId}/lan/{lanId}/seat/{seatId}/reservation", null);

    [Fact]
    public async Task succeeds_when_reserving_existing_available_seat()
    {
        // Arrange
		var guildId = CreateGuild();
        var identity = await CreateIdentity(guildId, Role.User);
        var client = GetClient(identity);

        var lan = LanGenerator.Create(guildId, CreateUser(guildId));
        var seat = SeatGenerator.Create(lan, CreateUser(lan.GuildId));

        await SetupAggregates(guildId, seat);

        //Act
        var response = await MakeRequest(client, guildId, lan.Id, seat.Id);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guildId).Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(identity.User.Id);
            });
    }

    [Fact]
    public async Task succeeds_when_reserving_seat_that_user_has_already_reserved()
    {
        // Arrange
		var guildId = CreateGuild();
        var identity = await CreateIdentity(guildId, Role.User);
        var client = GetClient(identity);

        var lan = LanGenerator.Create(guildId, CreateUser(guildId));
        var seat = SeatGenerator.Create(lan, CreateUser(lan.GuildId), reservedBy: identity.User);

        await SetupAggregates(guildId, seat);

        //Act
        var response = await MakeRequest(client, guildId, lan.Id, seat.Id);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guildId).Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(identity.User.Id);
            });
    }

    [Fact]
    public async Task fails_when_seat_is_already_reserved()
    {
        // Arrange
		var guildId = CreateGuild();
        var client = GetClient(guildId);

        var alreadyReservedBy = CreateUser(guildId);

        var lan = LanGenerator.Create(guildId, CreateUser(guildId));
        var seat = SeatGenerator.Create(lan, CreateUser(lan.GuildId), reservedBy: alreadyReservedBy);

        await SetupAggregates(guildId, seat);

        //Act
        var response = await MakeRequest(client, guildId, lan.Id, seat.Id);

        //Assert
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
		var guildId = CreateGuild();
        var client = GetClient(guildId);

        var lan = LanGenerator.Create(guildId, CreateUser(guildId));
        var seat = SeatGenerator.Create(lan, CreateUser(lan.GuildId));

        //Act
        var response = await MakeRequest(client, guildId, lan.Id, seat.Id);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task fails_when_user_already_has_a_reserved_seat()
    {
        // Arrange
		var guildId = CreateGuild();
        var identity = await CreateIdentity(guildId, Role.User);
        var client = GetClient(identity);

        var lan = LanGenerator.Create(guildId, CreateUser(guildId));
        var alreadyReservedSeat = SeatGenerator.Create(lan, CreateUser(lan.GuildId), reservedBy: identity.User);
        var seat = SeatGenerator.Create(lan, CreateUser(lan.GuildId));

        await SetupAggregates(guildId, alreadyReservedSeat, seat);

        //Act
        var response = await MakeRequest(client, guildId, lan.Id, seat.Id);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guildId).Should()
                    .ContainSingle(x => x.Id == seat.Id).Subject;
                committedSeat.ReservedBy.Should().BeNull();
            });
    }
}