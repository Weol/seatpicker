﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Entrypoints.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Reservation;

[ApiController]
[Route("api/lan/{lanId:Guid}/seat/{seatId:guid}/reservation")]
[Area("reservation")]
[Authorize]
public class CreateEndpoint
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromRoute] Guid lanId,
        [FromRoute] Guid seatId,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] IReservationService reservationService)
    {
        var user = await loggedInUserAccessor.Get();

        await reservationService.Create(lanId, seatId, user);

        return new OkResult();
    }
}