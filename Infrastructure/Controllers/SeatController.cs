using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Reservation;
using Seatpicker.Application.Features.Reservation.Ports;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Utils;

namespace Seatpicker.Infrastructure.Controllers;

[ApiController]
[Route("[controller]")]
public class SeatController
{
    private readonly IReservationService reservationService;
    private readonly ISeatRepository seatRepository;
    private readonly ILoggedInUserAccessor loggedInUserAccessor;

    public SeatController(IReservationService reservationService, ISeatRepository seatRepository, ILoggedInUserAccessor loggedInUserAccessor)
    {
        this.reservationService = reservationService;
        this.seatRepository = seatRepository;
        this.loggedInUserAccessor = loggedInUserAccessor;
    }

    [HttpGet]
    [ProducesResponseType(typeof(AllReservationsResponse), (int) HttpStatusCode.OK)]
    public async Task<IActionResult> Get()
    {
        var allReservations = await seatRepository.GetAll();
        return new OkObjectResult(new AllReservationsResponse(allReservations));
    }

    [HttpPost("reserve/{seatId:guid}")]
    [ProducesResponseType(typeof(Seat), (int) HttpStatusCode.OK)]
    [Authorize]
    public async Task<IActionResult> Reserve([FromRoute] Guid seatId)
    {
        var user = loggedInUserAccessor.Get();
        var seat = await reservationService.Reserve(user, seatId);

        return new OkObjectResult(seat);
    }

    [HttpDelete("unreserve/{seatId:guid}")]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    [Authorize]
    public async Task<IActionResult> Unreserve([FromRoute] Guid seatId)
    {
        var user = loggedInUserAccessor.Get();
        var response = reservationService.Reserve(user, seatId);

        return new OkResult();
    }

    private record ReservationNotFoundResponse(Guid SeatId);
    private record AllReservationsResponse(IEnumerable<Seat> Reservations);
}