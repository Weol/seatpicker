using System.Text;
using Seatpicker.Domain;

namespace Seatpicker.IntegrationTests.Tests;

public static class AggregateGenerator
{
    public static byte[] CreateValidBackround()
    {
        var svg = $"<svg>{Random.Shared.NextInt64().ToString()}</svg>";
        return Encoding.UTF8.GetBytes(svg);
    }

    public static Lan CreateLan(Guid? id = null, string? title = null, byte[]? background = null)
    {
        return new Lan(id ?? Guid.NewGuid(), title ?? "Test title", background ?? CreateValidBackround());
    }

    public static Seat CreateSeat(Guid? id = null, string? title = null, Bounds? bounds = null, User? reservedBy = null)
    {
        var seat = new Seat(id ?? Guid.NewGuid(), title ?? "Test title", bounds ?? new Bounds(0, 0, 1, 1));
        if (reservedBy is not null) seat.Reserve(reservedBy);
        return seat;
    }
}