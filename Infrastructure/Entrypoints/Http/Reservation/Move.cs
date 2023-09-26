using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Reservation;

public partial class ReservationController
{
    [HttpPut("{id:guid}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Move([FromRoute] Guid id, [FromBody] MoveReservationRequest model)
    {
        await validateModel.Validate<MoveReservationRequest, MoveReservationRequestValidator>(model);

        if (id != model.FromSeatId)
            throw new BadRequestException(
                $"Route parameter {nameof(id)} does not match the request model {nameof(MoveReservationRequest.FromSeatId)}");

        var user = loggedInUserAccessor.Get();

        await reservationService.Move(model.FromSeatId, model.ToSeatId, user);

        return new OkResult();
    }

    public record MoveReservationRequest(Guid FromSeatId, Guid ToSeatId);

    private class MoveReservationRequestValidator : AbstractValidator<MoveReservationRequest>
    {
        public MoveReservationRequestValidator()
        {
            RuleFor(x => x.FromSeatId).NotEmpty();

            RuleFor(x => x.ToSeatId).NotEmpty();
        }
    }
}