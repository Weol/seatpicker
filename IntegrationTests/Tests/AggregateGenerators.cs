using System.Text;
using Bogus;
using Seatpicker.Domain;

namespace Seatpicker.IntegrationTests.Tests;

public static class SeatGenerator
{
    public static Seat Create(
        Guid? id = null,
        string? title = null,
        Bounds? bounds = null,
        User? reservedBy = null,
        User? initiator = null)
    {
        var seat = new Seat(
            id ?? Guid.NewGuid(),
            title ?? "Test title",
            bounds ?? new Bounds(0, 0, 1, 1),
            initiator ?? new User(new UserId("124"), "asd"));

        if (reservedBy is not null) seat.MakeReservation(reservedBy, new List<Seat>());

        return seat;
    }
}

public static class LanGenerator
{
    public static byte[] CreateValidBackround()
    {
        var svg = $"<svg>{Random.Shared.NextInt64().ToString()}</svg>";
        return Encoding.UTF8.GetBytes(svg);
    }

    public static Lan Create(Guid? id = null, string? title = null, byte[]? background = null, User? initiator = null)
    {
        return new Lan(
            id ?? Guid.NewGuid(),
            title ?? "Test title",
            background ?? CreateValidBackround(),
            initiator ?? new User(new UserId("124"), "asd"));
    }
}