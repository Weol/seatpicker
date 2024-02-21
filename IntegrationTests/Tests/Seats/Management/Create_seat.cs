using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Authentication;
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
        MakeRequest(HttpClient client, Guid lanId, CreateSeat.Request request) =>
        await client.PostAsJsonAsync($"lan/{lanId}/seat", request);

    [Fact]
    public async Task succeeds_when_creating_new_seat()
    {
        // Arrange
        var client = GetClient(GuildId, Role.Operator);

        var lan = LanGenerator.Create(GuildId);
        await SetupAggregates(GuildId, lan);

        var model = Generator.CreateSeatRequest();

        //Act
        var response = await MakeRequest(client, lan.Id, model);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedDocuments<ProjectedSeat>(GuildId).Should().ContainSingle().Subject;
                committedSeat.Title.Should().Be(model.Title);
                committedSeat.Bounds.Should().BeEquivalentTo(model.Bounds);
            });
    }

    public static IEnumerable<object[]> InvalidUpdateRequests = new[]
    {
        new object[] { Generator.CreateSeatRequest() with { Title = "" } },
        new object[]
            { Generator.CreateSeatRequest() with { Bounds = new Bounds(0, 0, -1, 1) } },
        new object[]
            { Generator.CreateSeatRequest() with { Bounds = new Bounds(0, 0, 1, -1) } },
    };

    [Theory]
    [MemberData(nameof(InvalidUpdateRequests))]
    public async Task fails_when_seat_request_model_is_invalid(CreateSeat.Request request)
    {
        // Arrange
        var client = GetClient(GuildId, Role.Operator);

        //Act
        var response = await MakeRequest(client, Guid.NewGuid(), request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task fails_when_logged_in_user_has_insufficent_roles()
    {
        // Arrange
        var client = GetClient(GuildId);

        //Act
        var response = await MakeRequest(client, Guid.NewGuid(), Generator.CreateSeatRequest());

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}