using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Seat;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.Management;

// ReSharper disable once InconsistentNaming
public class Update_seat : IntegrationTestBase
{
    public Update_seat(
        TestWebApplicationFactory factory,
        PostgresFixture databaseFixture,
        ITestOutputHelper testOutputHelper) : base(factory, databaseFixture, testOutputHelper)
    {
    }

    private async Task<HttpResponseMessage> MakeRequest(
        HttpClient client,
        string guildId,
        Guid lanId,
        Guid seatId,
        UpdateSeat.Request request) =>
        await client.PutAsJsonAsync($"guild/{guildId}/lan/{lanId}/seat/{seatId}", request);

    [Fact]
    public async Task succeeds_when_valid()
    {
        // Arrange
        var request = Generator.UpdateSeatRequest();

        var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Operator);

        var lan = LanGenerator.Create(guildId, CreateUser(guildId));
        var existingSeat = SeatGenerator.Create(lan, CreateUser(lan.GuildId));

        await SetupAggregates(guildId, existingSeat);

        // Act
        var response = await MakeRequest(client, guildId, lan.Id, existingSeat.Id, request);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var seat = GetCommittedDocuments<ProjectedSeat>(guildId).Should().ContainSingle().Subject;
                Assert.Multiple(
                    () => seat.Title.Should().Be(seat.Title),
                    () => seat.Bounds.Should().BeEquivalentTo(seat.Bounds));
            });
    }

    [Fact]
    public async Task fails_when_seat_does_not_exists()
    {
        // Arrange
        var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Operator);

        var lan = LanGenerator.Create(guildId, CreateUser(guildId));
        await SetupAggregates(guildId, lan);

        // Act
        var response = await MakeRequest(client, guildId, lan.Id, Guid.NewGuid(), Generator.UpdateSeatRequest());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public static TheoryData<UpdateSeat.Request> InvalidUpdateRequests()
    {
        return new TheoryData<UpdateSeat.Request>
        {
            Generator.UpdateSeatRequest() with { Title = "" },
            Generator.UpdateSeatRequest() with
            {
                Bounds = new Seatpicker.Infrastructure.Entrypoints.Http.Seat.Bounds(0, 0, -1, 1)
            },
            Generator.UpdateSeatRequest() with
            {
                Bounds = new Seatpicker.Infrastructure.Entrypoints.Http.Seat.Bounds(0, 0, 1, -1)
            },
        };
    }

    [Theory]
    [MemberData(nameof(InvalidUpdateRequests))]
    public async Task fails_when_seat_request_model_is_invalid(UpdateSeat.Request request)
    {
        // Arrange
        var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Operator);

        // Act
        var response = await MakeRequest(client, guildId, Guid.NewGuid(), Guid.NewGuid(), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task fails_when_logged_in_user_has_insufficent_roles()
    {
        // Arrange
        var guildId = await CreateGuild();
        var client = GetClient(guildId);

        // Act
        var response = await MakeRequest(
            client,
            guildId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Generator.UpdateSeatRequest());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}