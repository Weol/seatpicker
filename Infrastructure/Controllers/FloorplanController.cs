using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Floorplan;
using Seatpicker.Application.Features.Reservation;
using Seatpicker.Application.Features.Reservation.Ports;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Utils;

namespace Seatpicker.Infrastructure.Controllers;

[ApiController]
[Route("[controller]")]
public class FloorplanController
{
    private readonly IFloorplanService floorplanService;

    public FloorplanController(IFloorplanService floorplanService)
    {
        this.floorplanService = floorplanService;
    }

    [HttpPut]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSeats([FromBody] ICollection<UpdateSeat> updateSeats)
    {
        await floorplanService.UpdateSeats(updateSeats);

        return new OkResult();
    }
}