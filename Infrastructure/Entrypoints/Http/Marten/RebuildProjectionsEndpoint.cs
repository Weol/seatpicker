using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lans;
using Seatpicker.Application.Features.Seats;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Marten;

[ApiController]
[Route("marten")]
[Authorize(Roles = "Admin")]
public class RebuildProjectionsEndpoint
{
    [HttpPost("reloadprojections")]
    [ProducesResponseType(200)]
    public async Task<ActionResult> Rebuild(
        [FromServices] IDocumentStore documentStore)
    {
        using var daemon = await documentStore.BuildProjectionDaemonAsync();

        await daemon.StartAllShards();

        await daemon.RebuildProjection<SeatProjection>(TimeSpan.FromMinutes(5), CancellationToken.None);
        await daemon.RebuildProjection<LanProjection>(TimeSpan.FromMinutes(5), CancellationToken.None);

        await daemon.StopAll();

        return new OkResult();
    }
}