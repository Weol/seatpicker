using Microsoft.Extensions.Logging;
using Seatpicker.Application.Features.Reservation.Ports;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Reservation;

public interface IReservationService
{
    public Task<ReservationResponse> Reserve(User user, int seatId);
}

internal class ReservationService : IReservationService
{
    private readonly ILogger<ReservationService> logger;
    private readonly IFloorplanRepository floorplanRepository;

    public async Task<ReservationResponse> Reserve(User user, int seatId)
    {
        var floorPlan = await floorplanRepository.Get();

        var seat = floorPlan.FindSeat(seatId);
        seat.User = user;

        floorplanRepository.Save(seat);
    }
}

public class ReservationResponse
{
}
