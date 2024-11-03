using Seatpicker.Domain;

namespace Seatpicker.IntegrationTests.Tests;

public static class SeatGenerator
{
    public static Seat Create(
        Lan lan,
        User user,
        User? reservedBy = null,
        string? id = null,
        string? title = null,
        Bounds? bounds = null)
    {
        var seat = new Seat(
            id ?? Guid.NewGuid().ToString(),
            lan,
            title ?? "Test title",
            bounds ?? new Bounds(0, 0, 1, 1),
            user);

        if (reservedBy is not null) seat.MakeReservation(reservedBy, 0);

        return seat;
    }
}

public static class LanGenerator
{

}