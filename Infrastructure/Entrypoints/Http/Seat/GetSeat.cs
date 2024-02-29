using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features;
using Seatpicker.Application.Features.Seats;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

public static class GetSeat
{
    public static async Task<IResult> GetAll(
        [FromRoute] Guid lanId,
        [FromServices] IDocumentRepository documentRepository,
        [FromServices] IUserProvider userProvider)
    {
        using var documentReader = documentRepository.CreateReader();

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

        return TypedResults.Ok(seats);
    }

    public static async Task<IResult> Get(
        [FromRoute] Guid lanId,
        [FromRoute] Guid seatId,
        [FromServices] IUserProvider userProvider,
        [FromServices] IDocumentRepository documentRepository)
    {
        using var documentReader = documentRepository.CreateReader();

        var seat = documentReader.Query<ProjectedSeat>()
            .Where(seat => seat.LanId == lanId)
            .SingleOrDefault(seat => seat.Id == seatId);

        if (seat is null) return TypedResults.NotFound();

        User? reservedBy = null;
        if (seat.ReservedBy is not null)
        {
            var user = await userProvider.GetById(seat.ReservedBy);
            if (user is not null)
            {
                reservedBy = new User(user.Id, user.Name, user.Avatar);
            }
        }

        return TypedResults.Ok(new Response(seat.Id, seat.Title, Bounds.FromDomainBounds(seat.Bounds), reservedBy));
    }

    public record Response(Guid Id, string Title, Bounds Bounds, User? ReservedBy);
}