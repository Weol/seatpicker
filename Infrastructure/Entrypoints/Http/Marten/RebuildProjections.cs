using Marten;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Application.Features.Reservation;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Marten;

public static class RebuildProjections
{
    public static async Task<IResult> Rebuild(
        [FromServices] IDocumentStore documentStore)
    {
        using var daemon = await documentStore.BuildProjectionDaemonAsync();

        await daemon.StartAllShards();

        await daemon.RebuildProjection<SeatProjection>(TimeSpan.FromMinutes(5), CancellationToken.None);
        await daemon.RebuildProjection<LanProjection>(TimeSpan.FromMinutes(5), CancellationToken.None);

        await daemon.StopAll();

        return TypedResults.Ok();
    }
}