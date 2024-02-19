using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

public class SeatNotFoundException : ApplicationException
{
    public required Guid SeatId { get; init; }

    protected override string ErrorMessage => $"Seat with id {SeatId} not found";
}

public class UserNotFoundException : ApplicationException
{
    public required string UserId { get; init; }

    protected override string ErrorMessage => $"User with id {UserId} not found";
}
