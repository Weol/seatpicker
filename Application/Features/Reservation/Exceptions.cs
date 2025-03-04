﻿namespace Seatpicker.Application.Features.Reservation;

public class SeatNotFoundException : ApplicationException
{
    public required string SeatId { get; init; }

    protected override string ErrorMessage => $"Seat with id {SeatId} not found";
}

public class UserNotFoundException : ApplicationException
{
    public required string UserId { get; init; }

    protected override string ErrorMessage => $"User with id {UserId} not found";
}
