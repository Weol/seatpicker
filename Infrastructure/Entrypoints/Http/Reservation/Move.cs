using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Reservation;

public partial class ReservationController
{
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Move([FromRoute] Guid id, [FromBody] MoveReservationRequestModel model)
    {
        await validateModel.Validate<MoveReservationRequestModel, MoveReservationRequestModelValidator>(model);

        if (id != model.FromSeatId)
            throw new BadRequestException(
                $"Route parameter {nameof(id)} does not match the request model {nameof(MoveReservationRequestModel.FromSeatId)}");

        var user = loggedInUserAccessor.Get();

        await reservationService.Move(model.FromSeatId, model.ToSeatId, user);

        return new OkResult();
    }

    public record MoveReservationRequestModel(Guid FromSeatId, Guid ToSeatId);

    private class MoveReservationRequestModelValidator : AbstractValidator<MoveReservationRequestModel>
    {
        public MoveReservationRequestModelValidator()
        {
            RuleFor(x => x.FromSeatId).NotEmpty();

            RuleFor(x => x.ToSeatId).NotEmpty();
        }
    }
}