using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;

public partial class ReservationManagementController
{
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Move([FromRoute] Guid id, [FromBody] MoveReservationForRequestModel model)
    {
        await validateModel.Validate<MoveReservationForRequestModel, MoveReservationForRequestModelValidator>(model);

        if (id != model.FromSeatId)
            throw new BadRequestException(
                $"Route parameter {nameof(id)} does not match the request model {nameof(MoveReservationForRequestModel.FromSeatId)}");

        var user = loggedInUserAccessor.Get();

        await reservationManagementService.Move(new UserId(model.UserId), model.FromSeatId, model.ToSeatId, user);

        return new OkResult();
    }

    public record MoveReservationForRequestModel(string UserId, Guid FromSeatId, Guid ToSeatId);

    private class MoveReservationForRequestModelValidator : AbstractValidator<MoveReservationForRequestModel>
    {
        public MoveReservationForRequestModelValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();

            RuleFor(x => x.FromSeatId).NotEmpty();

            RuleFor(x => x.ToSeatId).NotEmpty();
        }
    }
}