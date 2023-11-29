using FluentValidation;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lans;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints.Http.Lan;
using Seatpicker.Infrastructure.Entrypoints.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Marten;

[ApiController]
[Route("api/marten")]
[Area("marten")]
[Authorize(Roles = "Admin")]
public class RebuildProjectionsEndpoint
{
    [HttpPost]
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