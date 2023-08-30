﻿using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Seat;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.SeatManagement;

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
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedAggregates<Seat>().Should().ContainSingle().Subject;
                committedSeat.Id.Should().Be(model.SeatId);
                committedSeat.Title.Should().Be(model.Title);
                committedSeat.Bounds.Should().BeEquivalentTo(model.Bounds);
            });
    }

    [Fact]
    public async Task fails_when_seat_with_same_id_already_exists()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Operator);
        var client = GetClient(identity);

        var existingSeat = SeatGenerator.Create();

        SetupAggregates(existingSeat);

        var seat = SeatGenerator.Create(id: existingSeat.Id, title: "another title", initiator: identity.User);

        //Act
        var response = await client.PostAsJsonAsync(
            "seat",
            Generator.CreateSeatRequestModel() with { SeatId = existingSeat.Id });

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Conflict),
            () =>
            {
                var committedSeat = GetCommittedAggregates<Seat>().Should().ContainSingle().Subject;
                committedSeat.Should().NotBeEquivalentTo(seat);
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