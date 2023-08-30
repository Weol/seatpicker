using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Seat;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.SeatManagement;

// ReSharper disable once InconsistentNaming
public class Update_seat : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public Update_seat(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }

    public static IEnumerable<object[]> ValidUpdateRequestModels = new[]
    {
        new object[] { Generator.UpdateSeatRequestModel() },
        new object[]
        {
            Generator.UpdateSeatRequestModel() with { Title = null },
        },
        new object[]
        {
            Generator.UpdateSeatRequestModel() with { Bounds = null },
        },
    };

    [Theory]
    [MemberData(nameof(ValidUpdateRequestModels))]
    public async Task succeeds_when_valid(SeatController.UpdateSeatRequestModel model)
    {
        // Arrange
        var identity = await CreateIdentity(Role.Operator);
        var client = GetClient(identity);

        var existingSeat = SeatGenerator.Create();
        SetupAggregates(existingSeat);

        //Act
        var response = await client.PutAsJsonAsync($"seat/{existingSeat.Id}", model with { SeatId = existingSeat.Id });

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var seat = GetCommittedAggregates<Seat>().Should().ContainSingle().Subject;
                Assert.Multiple(
                    () =>
                    {
                        if (model.Title is not null) seat.Title.Should().Be(seat.Title);
                    },
                    () =>
                    {
                        if (model.Bounds is not null) seat.Bounds.Should().BeEquivalentTo(seat.Bounds);
                    });
            });
    }

    [Fact]
    public async Task fails_when_seat_does_not_exists()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Operator);
        var client = GetClient(identity);


        var model = Generator.CreateSeatRequestModel();

        //Act
        var response = await client.PutAsJsonAsync(
            $"seat/{model.SeatId}",
            model);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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