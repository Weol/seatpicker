using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

[ApiController]
[Route("seat")]
public class Remove
{
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Endpoint(
        [FromRoute] Guid id,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] ISeatManagementService seatManagementService)
    {
        var user = await loggedInUserAccessor.Get();

        await seatManagementService.Remove(id, user);

        return new OkResult();
    }
}