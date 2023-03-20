using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Floorplan;

namespace Seatpicker.Infrastructure.Entrypoints.Controllers;

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