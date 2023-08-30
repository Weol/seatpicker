using Bogus;
using Seatpicker.Infrastructure.Entrypoints.Http.Seat;

namespace Seatpicker.IntegrationTests.Tests.Seats.SeatManagement;

public static class Generator
{
    public static SeatController.CreateSeatRequestModel CreateSeatRequestModel()
    {
        return new SeatController.CreateSeatRequestModel(Guid.NewGuid(), Title:
            new Faker().Hacker.Verb(), new SeatController.BoundsModel(0, 0, 1, 1));
    }

    public static SeatController.UpdateSeatRequestModel UpdateSeatRequestModel()
    {
        return new SeatController.UpdateSeatRequestModel(Guid.NewGuid(), Title:
            new Faker().Hacker.Verb(), new SeatController.BoundsModel(0, 0, 1, 1));
    }
}