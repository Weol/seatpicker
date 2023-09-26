using Bogus;
using Seatpicker.Infrastructure.Entrypoints.Http.Seat;

namespace Seatpicker.IntegrationTests.Tests.Seats.Management;

public static class Generator
{
    public static SeatController.CreateSeatRequest CreateSeatRequest()
    {
        return new SeatController.CreateSeatRequest(
            Title: new Faker().Hacker.Verb(),
            new SeatController.BoundsModel(0, 0, 1, 1));
    }

    public static SeatController.UpdateSeatRequest UpdateSeatRequest()
    {
        return new SeatController.UpdateSeatRequest(
            Guid.NewGuid(),
            Title: new Faker().Hacker.Verb(),
            new SeatController.BoundsModel(0, 0, 1, 1));
    }
}