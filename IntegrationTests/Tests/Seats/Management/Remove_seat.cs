using System.Diagnostics.CodeAnalysis;
using System.Net;
using FluentAssertions;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Domain;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.Management;

// ReSharper disable once InconsistentNaming
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class Remove_seat : IntegrationTestBase
{
    public Remove_seat(TestWebApplicationFactory factory, PostgresFixture databaseFixture, ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    private static async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId, Guid lanId, Guid seatId) =>
        await client.DeleteAsync($"guild/{guildId}/lan/{lanId}/seat/{seatId}");

    [Fact]
    public async Task succeeds_when_seat_exists()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Operator);

        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var seat = SeatGenerator.Create(lan, CreateUser(lan.GuildId));
        await SetupAggregates(guild.Id, seat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, seat.Id);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        GetCommittedDocuments<ProjectedSeat>(guild.Id).Should().BeEmpty();
    }

    [Fact]
    public async Task fails_when_seat_does_not_exists()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Operator);

        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        await SetupAggregates(guild.Id, lan);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, Guid.NewGuid());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task fails_when_logged_in_user_has_insufficent_roles()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id);

        // Act
        var response = await MakeRequest(client, guild.Id, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}