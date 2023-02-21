using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Reservation;

namespace Seatpicker.Infrastructure.Controllers;

[ApiController]
[Route("[controller]")]
public class ReservationController
{
    private readonly IReservationService reservationService;

    public ReservationController(IReservationService reservationService)
    {
        this.reservationService = reservationService;
    }

    [HttpPut("{seatId:guid}")]
    [Authorize]
    public async Task<IActionResult> Put([FromRoute] Guid seatId)
    {
        var response = reservationService.Reserve(seatId);
    }
}