using System.Net;
using FluentAssertions;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Domain;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.Management;

// ReSharper disable once InconsistentNaming
public class Remove_seat : IntegrationTestBase
{
    public Remove_seat(TestWebApplicationFactory factory, PostgresFixture databaseFixture, ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId, Guid lanId, Guid seatId) =>
        await client.DeleteAsync($"guild/{guildId}/lan/{lanId}/seat/{seatId}");

    [Fact]
    public async Task succeeds_when_seat_exists()
    {
        // Arrange
		var guildId = CreateGuild();
        var client = GetClient(guildId, Role.Operator);

        var lan = LanGenerator.Create(guildId);
        var seat = SeatGenerator.Create(lan);
        await SetupAggregates(guildId, seat);

        //Act
        var response = await MakeRequest(client, guildId, lan.Id, seat.Id);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        GetCommittedDocuments<ProjectedSeat>(guildId).Should().BeEmpty();
    }

    [Fact]
    public async Task fails_when_seat_does_not_exists()
    {
        // Arrange
		var guildId = CreateGuild();
        var client = GetClient(guildId, Role.Operator);

        var lan = LanGenerator.Create(guildId);
        await SetupAggregates(guildId, lan);

        //Act
        var response = await MakeRequest(client, guildId, lan.Id, Guid.NewGuid());

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task fails_when_logged_in_user_has_insufficent_roles()
    {
        // Arrange
		var guildId = CreateGuild();
        var client = GetClient(guildId);

        //Act
        var response = await MakeRequest(client, guildId, Guid.NewGuid(), Guid.NewGuid());

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}