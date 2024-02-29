using Bogus;
using Seatpicker.Infrastructure.Entrypoints.Http.Seat;

namespace Seatpicker.IntegrationTests.Tests.Seats.Management;

public static class Generator
{
    public static CreateSeat.Request CreateSeatRequest()
    {
        return new CreateSeat.Request(
            Title: new Faker().Hacker.Verb(),
            Bounds: new Bounds(0, 0, 1, 1));
    }

    public static UpdateSeat.Request UpdateSeatRequest()
    {
        return new UpdateSeat.Request(
            Title: new Faker().Hacker.Verb(),
            new Bounds(0, 0, 1, 1));
    }
}