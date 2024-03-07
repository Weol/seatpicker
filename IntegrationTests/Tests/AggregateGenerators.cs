using System.Text;
using Seatpicker.Domain;

namespace Seatpicker.IntegrationTests.Tests;

public static class SeatGenerator
{
    public static Seat Create(
        Lan lan,
        User initiator,
        User? reservedBy = null,
        Guid? id = null,
        string? title = null,
        Bounds? bounds = null)
    {
        var seat = new Seat(
            id ?? Guid.NewGuid(),
            lan,
            title ?? "Test title",
            bounds ?? new Bounds(0, 0, 1, 1),
            initiator);

        if (reservedBy is not null) seat.MakeReservation(reservedBy, 0);

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

    public static Lan Create(string guildId, User initiator, Guid? id = null, string? title = null, byte[]? background = null)
    {
        return new Lan(
            id ?? Guid.NewGuid(),
            guildId,
            title ?? "Test title",
            background ?? CreateValidBackround(),
            initiator);
    }
}