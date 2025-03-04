﻿using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

public record SeatResponse(string Id, string Title, Bounds Bounds, User? ReservedB, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);