using System.Net;
using FluentAssertions;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Authentication;
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

    private async Task<HttpResponseMessage> MakeRequest(HttpClient client, Guid lanId, Guid seatId) =>
        await client.DeleteAsync($"lan/{lanId}/seat/{seatId}");

    [Fact]
    public async Task succeeds_when_seat_exists()
    {
        // Arrange
        var client = GetClient(Role.Operator);

        var lan = LanGenerator.Create(GuildId);
        var seat = SeatGenerator.Create(lan);
        await SetupAggregates(seat);

        //Act
        var response = await MakeRequest(client, lan.Id, seat.Id);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        GetCommittedDocuments<ProjectedSeat>().Should().BeEmpty();
    }

    [Fact]
    public async Task fails_when_seat_does_not_exists()
    {
        // Arrange
        var client = GetClient(Role.Operator);

        var lan = LanGenerator.Create(GuildId);
        await SetupAggregates(lan);

        //Act
        var response = await MakeRequest(client, lan.Id, Guid.NewGuid());

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task fails_when_logged_in_user_has_insufficent_roles()
    {
        // Arrange
        var client = GetClient();

        //Act
        var response = await MakeRequest(client, Guid.NewGuid(), Guid.NewGuid());

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}