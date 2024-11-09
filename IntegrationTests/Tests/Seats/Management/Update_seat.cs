using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Application.Features.Reservation;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Seat;
using Xunit;
using Xunit.Abstractions;
using Bounds = Seatpicker.Infrastructure.Entrypoints.Http.Seat.Bounds;

namespace Seatpicker.IntegrationTests.Tests.Seats.Management;

// ReSharper disable once InconsistentNaming
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class Update_seat(
    TestWebApplicationFactory factory,
    PostgresFixture databaseFixture,
    ITestOutputHelper testOutputHelper) : IntegrationTestBase(factory, databaseFixture, testOutputHelper)
{
    private static async Task<HttpResponseMessage> MakeRequest(
        HttpClient client,
        string guildId,
        string lanId,
        string seatId,
        UpdateSeat.Request request) =>
        await client.PutAsJsonAsync($"guild/{guildId}/lan/{lanId}/seat/{seatId}", request);

    [Fact]
    public async Task succeeds_when_valid()
    {
        // Arrange
        var request = UpdateSeatRequest();

        var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Operator);

        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        var existingSeat = SeatGenerator.Create(lan, CreateUser(guild.Id));

        await SetupAggregates(guild.Id, existingSeat);

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, existingSeat.Id, request);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var seat = GetCommittedDocuments<ProjectedSeat>(guild.Id).Should().ContainSingle().Subject;
                Assert.Multiple(
                    () => seat.Title.Should().Be(seat.Title),
                    () => seat.Bounds.Should().BeEquivalentTo(seat.Bounds));
            });
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
        var response = await MakeRequest(client, guild.Id, lan.Id, Guid.NewGuid().ToString(), UpdateSeatRequest());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public static TheoryData<UpdateSeat.Request> InvalidUpdateRequests()
    {
        return new TheoryData<UpdateSeat.Request> {
            UpdateSeatRequest() with { Title = "" },
            UpdateSeatRequest() with { Bounds = new Bounds(0, 0, -1, 1) },
            UpdateSeatRequest() with { Bounds = new Bounds(0, 0, 1, -1) },
        };
    }

    [Theory]
    [MemberData(nameof(InvalidUpdateRequests))]
    public async Task fails_when_seat_request_model_is_invalid(UpdateSeat.Request request)
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Operator);

        // Act
        var response = await MakeRequest(client, guild.Id, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task fails_when_logged_in_user_has_insufficent_roles()
    {
        // Arrange
        var guild = await CreateGuild();
        var client = GetClient(guild.Id);

        // Act
        var response = await MakeRequest(
            client,
            guild.Id,
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            UpdateSeatRequest());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static UpdateSeat.Request UpdateSeatRequest()
    {
        return new UpdateSeat.Request(
            Title: RandomData.Faker.Hacker.Verb(),
            new Bounds(0, 0, 1, 1));
    }
}