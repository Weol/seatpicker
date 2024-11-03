using System.Diagnostics.CodeAnalysis;
using System.Net;
using FluentAssertions;
using Seatpicker.Application.Features.Reservation;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.Reservation;

// ReSharper disable once InconsistentNaming
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class Remove_reservation(
    TestWebApplicationFactory factory,
    PostgresFixture databaseFixture,
    ITestOutputHelper testOutputHelper) : IntegrationTestBase(factory, databaseFixture, testOutputHelper)
{
    private static async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId, Guid lanId, Guid seatId) =>
        await client.DeleteAsync($"guild/{guildId}/lan/{lanId}/seat/{seatId}/reservation");

    [Fact]
    public async Task succeeds_when_seat_is_reserved_by_user()
    {
        // Arrange
		var guild = await CreateGuild();
        var identity = await CreateIdentity(guild.Id);
        var client = GetClient(identity);

        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var seat = SeatGenerator.Create(lan, CreateUser(lan.GuildId), reservedBy: identity.User);

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
        var identity = await CreateIdentity(guild.Id);
        var client = GetClient(identity);

        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var seat = SeatGenerator.Create(lan, CreateUser(lan.GuildId));

        await SetupAggregates(guild.Id, seat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, seat.Id);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task fails_when_seat_is_reserved_by_different_user()
    {
        // Arrange
		var guild = await CreateGuild();
        var identity = await CreateIdentity(guild.Id);
        var client = GetClient(identity);

        var alreadyReservedBy = CreateUser(guild.Id);
        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var seat = SeatGenerator.Create(lan, CreateUser(lan.GuildId), reservedBy: alreadyReservedBy);

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
        var identity = await CreateIdentity(guild.Id);
        var client = GetClient(identity);

        // Act
        var response = await client.DeleteAsync($"reservation/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}