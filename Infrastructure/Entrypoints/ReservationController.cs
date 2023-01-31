using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Seatpicker.Infrastructure.Entrypoints;

[ApiController]
[Route("[controller]")]
public class ReservationController
{
    [HttpPut("{seatId:guid}")]
    [Authorize]
    public async Task<IActionResult> Put(Guid seatId)
    {
        return new OkObjectResult("Hehe");
    }
}