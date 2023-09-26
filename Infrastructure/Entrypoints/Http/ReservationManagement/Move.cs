using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;

public partial class ReservationManagementController
{
    [HttpPut("{id:guid}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Move([FromRoute] Guid id, [FromBody] MoveReservationForRequest model)
    {
        await validateModel.Validate<MoveReservationForRequest, MoveReservationForRequestValidator>(model);

        if (id != model.FromSeatId)
            throw new BadRequestException(
                $"Route parameter {nameof(id)} does not match the request model {nameof(MoveReservationForRequest.FromSeatId)}");

        var user = loggedInUserAccessor.Get();

        await reservationManagementService.Move(new UserId(model.UserId), model.FromSeatId, model.ToSeatId, user);

        return new OkResult();
    }

    public record MoveReservationForRequest(string UserId, Guid FromSeatId, Guid ToSeatId);

    private class MoveReservationForRequestValidator : AbstractValidator<MoveReservationForRequest>
    {
        public MoveReservationForRequestValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();

            RuleFor(x => x.FromSeatId).NotEmpty();

            RuleFor(x => x.ToSeatId).NotEmpty();
        }
    }
}