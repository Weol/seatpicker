using System.Diagnostics.CodeAnalysis;
using System.Net;
using FluentAssertions;
using Seatpicker.Application.Features.Reservation;
using Seatpicker.Domain;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.Reservation;

// ReSharper disable once InconsistentNaming
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class Create_reservation(
    TestWebApplicationFactory factory,
    PostgresFixture databaseFixture,
    ITestOutputHelper testOutputHelper) : IntegrationTestBase(factory, databaseFixture, testOutputHelper)
{
    private static async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId, string lanId, string seatId) =>
        await client.PostAsync($"guild/{guildId}/lan/{lanId}/seat/{seatId}/reservation", null);

    [Fact]
    public async Task succeeds_when_reserving_existing_available_seat()
    {
        // Arrange
		var guild = await CreateGuild();
        var identity = await CreateIdentity(guild.Id, Role.User);
        var client = GetClient(identity);

        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var seat = SeatGenerator.Create(lan, CreateUser(guild.Id));

        await SetupAggregates(guild.Id, seat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, seat.Id);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guild.Id).Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(identity.User.Id);
            });
    }

    [Fact]
    public async Task succeeds_when_reserving_seat_that_user_has_already_reserved()
    {
        // Arrange
		var guild = await CreateGuild();
        var identity = await CreateIdentity(guild.Id, Role.User);
        var client = GetClient(identity);

        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var seat = SeatGenerator.Create(lan, CreateUser(guild.Id), reservedBy: identity.User);

        await SetupAggregates(guild.Id, seat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, seat.Id);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guild.Id).Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(identity.User.Id);
            });
    }

    [Fact]
    public async Task fails_when_seat_is_already_reserved()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id);

        var alreadyReservedBy = CreateUser(guild.Id);

        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var seat = SeatGenerator.Create(lan, CreateUser(guild.Id), reservedBy: alreadyReservedBy);

        await SetupAggregates(guild.Id, seat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, seat.Id);

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
        var client = GetClient(guild.Id);

        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var seat = SeatGenerator.Create(lan, CreateUser(guild.Id));

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, seat.Id);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task fails_when_user_already_has_a_reserved_seat()
    {
        // Arrange
		var guild = await CreateGuild();
        var identity = await CreateIdentity(guild.Id, Role.User);
        var client = GetClient(identity);

        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var alreadyReservedSeat = SeatGenerator.Create(lan, CreateUser(guild.Id), reservedBy: identity.User);
        var seat = SeatGenerator.Create(lan, CreateUser(guild.Id));

        await SetupAggregates(guild.Id, alreadyReservedSeat, seat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, seat.Id);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guild.Id).Should()
                    .ContainSingle(x => x.Id == seat.Id).Subject;
                committedSeat.ReservedBy.Should().BeNull();
            });
    }
}