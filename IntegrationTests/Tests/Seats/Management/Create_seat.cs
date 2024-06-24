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
public class Create_seat : IntegrationTestBase
{
    public Create_seat(TestWebApplicationFactory factory, PostgresFixture databaseFixture, ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    private static async Task<HttpResponseMessage> MakeRequest(HttpClient client, string guildId, Guid lanId, CreateSeat.Request request) =>
        await client.PostAsJsonAsync($"guild/{guildId}/lan/{lanId}/seat", request);

    [Fact]
    public async Task succeeds_when_creating_new_seat()
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Operator);

        var lan = RandomData.Aggregates.Lan(guild.Id, CreateUser(guild.Id));
        await SetupAggregates(guild.Id, lan);

        var model = CreateSeatRequest();

        // Act
        var response = await MakeRequest(client, guild.Id, lan.Id, model);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guild.Id).Should().ContainSingle().Subject;
                committedSeat.Title.Should().Be(model.Title);
                committedSeat.Bounds.Should().BeEquivalentTo<Bounds>(model.Bounds);
            });
    }

    public static TheoryData<CreateSeat.Request> InvalidUpdateRequests()
    {
        return new TheoryData<CreateSeat.Request>
        {
            CreateSeatRequest() with { Title = "" },
            CreateSeatRequest() with
            {
                Bounds = new Bounds(0, 0, -1, 1)
            },
            CreateSeatRequest() with
            {
                Bounds = new Bounds(0, 0, 1, -1)
            },
        };
    }

    [Theory]
    [MemberData(nameof(InvalidUpdateRequests))]
    public async Task fails_when_seat_request_model_is_invalid(CreateSeat.Request request)
    {
        // Arrange
		var guild = await CreateGuild();
        var client = GetClient(guild.Id, Role.Operator);

        // Act
        var response = await MakeRequest(client, guild.Id, Guid.NewGuid(), request);

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
        var response = await MakeRequest(client, guild.Id, Guid.NewGuid(), CreateSeatRequest());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static CreateSeat.Request CreateSeatRequest()
    {
        return new CreateSeat.Request(
            Title: RandomData.Faker.Hacker.Verb(),
            Bounds: new Bounds(0, 0, 1, 1));
    }
}