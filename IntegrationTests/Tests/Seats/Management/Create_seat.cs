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
public class Create_seat : IntegrationTestBase
{
    public Create_seat(TestWebApplicationFactory factory, PostgresFixture databaseFixture, ITestOutputHelper testOutputHelper) : base(
        factory,
        databaseFixture,
        testOutputHelper)
    {
    }

    private async Task<HttpResponseMessage>
        MakeRequest(HttpClient client, string guildId, Guid lanId, CreateSeat.Request request) =>
        await client.PostAsJsonAsync($"guild/{guildId}/lan/{lanId}/seat", request);

    [Fact]
    public async Task succeeds_when_creating_new_seat()
    {
        // Arrange
		var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Operator);

        var lan = LanGenerator.Create(guildId, CreateUser(guildId));
        await SetupAggregates(guildId, lan);

        var model = Generator.CreateSeatRequest();

        // Act
        var response = await MakeRequest(client, guildId, lan.Id, model);

        // Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(guildId).Should().ContainSingle().Subject;
                committedSeat.Title.Should().Be(model.Title);
                committedSeat.Bounds.Should().BeEquivalentTo(model.Bounds);
            });
    }

    public static TheoryData<CreateSeat.Request> InvalidUpdateRequests()
    {
        return new TheoryData<CreateSeat.Request>
        {
            Generator.CreateSeatRequest() with { Title = "" },
            Generator.CreateSeatRequest() with
            {
                Bounds = new Seatpicker.Infrastructure.Entrypoints.Http.Seat.Bounds(0, 0, -1, 1)
            },
            Generator.CreateSeatRequest() with
            {
                Bounds = new Seatpicker.Infrastructure.Entrypoints.Http.Seat.Bounds(0, 0, 1, -1)
            },
        };
    }

    [Theory]
    [MemberData(nameof(InvalidUpdateRequests))]
    public async Task fails_when_seat_request_model_is_invalid(CreateSeat.Request request)
    {
        // Arrange
		var guildId = await CreateGuild();
        var client = GetClient(guildId, Role.Operator);

        // Act
        var response = await MakeRequest(client, guildId, Guid.NewGuid(), request);

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
        var response = await MakeRequest(client, guildId, Guid.NewGuid(), Generator.CreateSeatRequest());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}