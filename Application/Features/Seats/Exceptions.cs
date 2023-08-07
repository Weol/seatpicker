using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

public class SeatNotFoundException : ApplicationException
{
    public required Guid SeatId { get; init; }

    public override string Message => $"Seat with id {SeatId} not found";
}