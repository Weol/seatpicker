using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

[ApiController]
[Route("api/lan/{lanId:guid}/seat")]
[Area("seat")]
public class GetEndpoint
{
    [HttpGet("")]
    public async Task<ActionResult<Response[]>> GetAll(
        [FromRoute] Guid lanId,
        [FromServices] IDocumentReader documentReader,
        [FromServices] IUserProvider userProvider)
    {
        var tasks = documentReader.Query<ProjectedSeat>()
            .Where(seat => seat.LanId == lanId)
            .AsEnumerable()
            .Select(
                async seat =>
                {
                    User? reservedBy = null;
                    if (seat.ReservedBy is not null)
                    {
                        var user = await userProvider.GetById(seat.ReservedBy);
                        if (user is not null)
                        {
                            reservedBy = new User(user.Id, user.Name, user.Avatar);
                        }
                    }

                    return new Response(seat.Id, seat.Title, Bounds.FromDomainBounds(seat.Bounds), reservedBy);
                });

        var seats = await Task.WhenAll(tasks);

        return new OkObjectResult(seats);
    }

    [HttpGet("{seatId:guid}")]
    public async Task<ActionResult<Response>> Get(
        [FromRoute] Guid lanId,
        [FromRoute] Guid seatId,
        [FromServices] IUserProvider userProvider,
        [FromServices] IDocumentReader documentReader)
    {
        var seat = documentReader.Query<ProjectedSeat>()
            .Where(seat => seat.LanId == lanId)
            .SingleOrDefault(seat => seat.Id == seatId);

        if (seat is null) return new NotFoundResult();

        User? reservedBy = null;
        if (seat.ReservedBy is not null)
        {
            var user = await userProvider.GetById(seat.ReservedBy);
            if (user is not null)
            {
                reservedBy = new User(user.Id, user.Name, user.Avatar);
            }
        }

        return new OkObjectResult(new Response(seat.Id, seat.Title, Bounds.FromDomainBounds(seat.Bounds), reservedBy));
    }

    public record Response(Guid Id, string Title, Bounds Bounds, User? ReservedBy);
}