using Microsoft.AspNetCore.Mvc;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

public partial class SeatController
{
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Remove([FromRoute] Guid id)
    {
        var user = loggedInUserAccessor.Get();

        await seatManagementService.Remove(id, user);

        return new OkResult();
    }
}