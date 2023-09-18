using Microsoft.AspNetCore.Mvc;

namespace Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;

public partial class ReservationManagementController
{
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remove([FromRoute] Guid id)
    {
        var user = loggedInUserAccessor.Get();

        await reservationManagementService.Remove(id, user);

        return new OkResult();
    }
}