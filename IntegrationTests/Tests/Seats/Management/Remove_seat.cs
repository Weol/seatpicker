using System.Net;
using FluentAssertions;
using Seatpicker.Domain;
using Xunit;
using Xunit.Abstractions;

namespace Seatpicker.IntegrationTests.Tests.Seats.Management;

// ReSharper disable once InconsistentNaming
public class Remove_seat : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public Remove_seat(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(
        factory,
        testOutputHelper)
    {
    }

    [Fact]
    public async Task succeeds_when_seat_exists()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Operator);
        var client = GetClient(identity);

        var seat = SeatGenerator.Create();
        SetupAggregates(seat);

        //Act
        var response = await client.DeleteAsync($"seat/{seat.Id}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        GetCommittedAggregates<Seat>().Should().BeEmpty();
    }

    [Fact]
    public async Task fails_when_seat_does_not_exists()
    {
        // Arrange
        var identity = await CreateIdentity(Role.Operator);
        var client = GetClient(identity);

        //Act
        var response = await client.DeleteAsync(
            $"seat/{Guid.NewGuid()}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}