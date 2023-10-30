﻿using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Reservation;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.Reservation;

// ReSharper disable once InconsistentNaming
public class Create_reservation : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public Create_reservation(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }

    [Fact]
    public async Task succeeds_when_reserving_existing_available_seat()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var seat = SeatGenerator.Create();

        SetupAggregates(seat);

        //Act
        var response = await client.PostAsync(
            "reservation",
            JsonContent.Create(new CreateEndpoint.Request(seat.Id)));

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedAggregates<Seat>().Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(identity.User.Id);
            });
    }

    [Fact]
    public async Task succeeds_when_reserving_seat_that_user_has_already_reserved()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var seat = SeatGenerator.Create(reservedBy: identity.User);

        SetupAggregates(seat);

        //Act
        var response = await client.PostAsJsonAsync(
            "reservation",
            new CreateEndpoint.Request(seat.Id));

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.OK),
            () =>
            {
                var committedSeat = GetCommittedAggregates<Seat>().Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(identity.User.Id);
            });
    }

    [Fact]
    public async Task fails_when_seat_is_already_reserved()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var alreadyReservedBy = await CreateUser();

        var seat = SeatGenerator.Create(reservedBy: alreadyReservedBy);

        SetupAggregates(seat);

        //Act
        var response = await client.PostAsJsonAsync(
            "reservation",
            new CreateEndpoint.Request(seat.Id));

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.Conflict),
            () =>
            {
                var committedSeat = GetCommittedAggregates<Seat>().Should().ContainSingle().Subject;
                committedSeat.ReservedBy.Should().NotBeNull();
                committedSeat.ReservedBy!.Should().Be(alreadyReservedBy.Id);
            });
    }

    [Fact]
    public async Task fails_when_seat_does_not_exist()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var seat = SeatGenerator.Create();

        //Act
        var response = await client.PostAsJsonAsync(
            "reservation",
            new CreateEndpoint.Request(seat.Id));

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task fails_when_user_already_has_a_reserved_seat()
    {
        // Arrange
        var identity = await CreateIdentity();
        var client = GetClient(identity);

        var alreadyReservedSeat = SeatGenerator.Create(reservedBy: identity.User);
        var seat = SeatGenerator.Create();

        SetupAggregates(alreadyReservedSeat, seat);

        //Act
        var response = await client.PostAsJsonAsync(
            "reservation",
            new CreateEndpoint.(seat.Id));

        //Assert
        Assert.Multiple(
            () => response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity),
            () =>
            {
                var committedSeat = GetCommittedAggregates<Seat>().Should().ContainSingle(x => x.Id == seat.Id).Subject;
                committedSeat.ReservedBy.Should().BeNull();
            });
    }
}