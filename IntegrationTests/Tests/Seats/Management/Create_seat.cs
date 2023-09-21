using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints.Http.Seat;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.Management;

// ReSharper disable once InconsistentNaming
public class Create_seat : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public Create_seat(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }

    [Fact]
    public async Task succeeds_when_creating_new_seat()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Operator);
        var client = GetClient(identity);

        var model = Generator.CreateSeatRequestModel();

        //Act
        var response = await client.PostAsJsonAsync("seat", model);

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Created),
            () =>
            {
                var committedSeat = GetCommittedAggregates<Seat>().Should().ContainSingle().Subject;
                committedSeat.Title.Should().Be(model.Title);
                committedSeat.Bounds.Should().BeEquivalentTo(model.Bounds);
            });
    }

    public static IEnumerable<object[]> InvalidUpdateRequestModels = new[]
    {
        new object[] { Generator.CreateSeatRequestModel() with { Title = "" } },
        new object[]
            { Generator.CreateSeatRequestModel() with { Bounds = new SeatController.BoundsModel(0, 0, -1, 1) } },
        new object[]
            { Generator.CreateSeatRequestModel() with { Bounds = new SeatController.BoundsModel(0, 0, 1, -1) } },
    };

    [Theory]
    [MemberData(nameof(InvalidUpdateRequestModels))]
    public async Task fails_when_seat_request_model_is_invalid(SeatController.CreateSeatRequestModel model)
    {
        // Arrange
        var identity = await CreateIdentity(Role.Operator);
        var client = GetClient(identity);

        //Act
        var response = await client.PostAsJsonAsync("seat", model);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}