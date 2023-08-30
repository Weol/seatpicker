using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

public partial class SeatController
{
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateSeatRequestModel model)
    {
        await validateModel.Validate<UpdateSeatRequestModel, UpdateSeatRequestModelModelValidator>(model);

        if (id != model.SeatId)
            throw new BadRequestException(
                $"Route parameter {nameof(id)} does not match the request model {nameof(UpdateSeatRequestModel.SeatId)}");

        var user = loggedInUserAccessor.Get();

        await seatManagementService.Update(model.SeatId, model.Title, model.Bounds?.ToDomainBounds(), user);

        return new OkResult();
    }

    public record UpdateSeatRequestModel(Guid SeatId, string? Title, BoundsModel? Bounds);

    private class UpdateSeatRequestModelModelValidator : AbstractValidator<UpdateSeatRequestModel>
    {
        public UpdateSeatRequestModelModelValidator()
        {
            RuleFor(x => x.SeatId).NotEmpty();

            RuleFor(x => x.Title)
                .NotEmpty()
                .When(x => x.Title is not null);

            RuleFor(x => x.Bounds)
                .SetValidator(new BoundsModelValidator()!)
                .When(x => x.Bounds is not null);
        }
    }
}