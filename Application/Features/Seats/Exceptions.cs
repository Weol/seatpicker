using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

public class SeatNotFoundException : ApplicationException
{
    public required Guid SeatId { get; init; }

    protected override string ErrorMessage => $"Seat with id {SeatId} not found";
}

