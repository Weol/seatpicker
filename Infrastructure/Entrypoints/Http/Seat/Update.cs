using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

public partial class SeatController
{
    [HttpPut("{id:guid}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateSeatRequest model)
    {
        await validateModel.Validate<UpdateSeatRequest, UpdateSeatRequestModelValidator>(model);

        if (id != model.SeatId)
            throw new BadRequestException(
                $"Route parameter {nameof(id)} does not match the request model {nameof(UpdateSeatRequest.SeatId)}");

        var user = loggedInUserAccessor.Get();

        await seatManagementService.Update(model.SeatId, model.Title, model.Bounds?.ToDomainBounds(), user);

        return new OkResult();
    }

    public record UpdateSeatRequest(Guid SeatId, string? Title, BoundsModel? Bounds);

    private class UpdateSeatRequestModelValidator : AbstractValidator<UpdateSeatRequest>
    {
        public UpdateSeatRequestModelValidator()
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