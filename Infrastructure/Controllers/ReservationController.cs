using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Reservation;
using Seatpicker.Application.Features.Reservation.Ports;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Utils;

namespace Seatpicker.Infrastructure.Controllers;

[ApiController]
[Route("[controller]")]
public class ReservationController
{
    private readonly IReservationService reservationService;
    private readonly ISeatRepository seatRepository;
    private readonly ILoggedInUserAccessor loggedInUserAccessor;

    public ReservationController(IReservationService reservationService, ISeatRepository seatRepository, ILoggedInUserAccessor loggedInUserAccessor)
    {
        this.reservationService = reservationService;
        this.seatRepository = seatRepository;
        this.loggedInUserAccessor = loggedInUserAccessor;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var allReservations = await seatRepository.GetAll();
        return new OkObjectResult(new AllReservationsResponse(allReservations));
    }

    [HttpPut("{seatId:guid}")]
    [Authorize]
    public async Task<IActionResult> Put([FromRoute] Guid seatId)
    {
        var user = loggedInUserAccessor.Get();
        var response = reservationService.Reserve(user, seatId);

        return new OkResult();
    }

    [HttpDelete("{seatId:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete([FromRoute] Guid seatId)
    {
        var user = loggedInUserAccessor.Get();
        var response = reservationService.Reserve(user, seatId);

        return new OkResult();
    }

    private record AllReservationsResponse(IEnumerable<Reservation> Reservations);
}