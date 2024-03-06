﻿using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

public static class CreateSeat
{
    public static async Task<IResult> Create(
        [FromRoute] string guildId,
        [FromRoute] Guid lanId,
        [FromBody] Request request,
        [FromServices] ILoggedInUserAccessor loggedInUserAccessor,
        [FromServices] ISeatManagementService seatManagementService)
    {
        var user = await loggedInUserAccessor.Get();

        var id = await seatManagementService.Create(lanId, request.Title, request.Bounds.ToDomainBounds(), user);

        return TypedResults.Ok(new Response(id));
    }

    public record Request(string Title, Bounds Bounds);

    public record Response(Guid Id);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty();

            RuleFor(x => x.Bounds).SetValidator(new BoundsValidator());
        }
    }
}