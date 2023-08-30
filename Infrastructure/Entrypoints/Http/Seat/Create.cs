using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

public partial class SeatController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSeatRequestModel model)
    {
        await validateModel.Validate<CreateSeatRequestModel, CreateSeatRequestModelModelValidator>(model);

        var user = loggedInUserAccessor.Get();

        await seatManagementService.Create(
            model.SeatId,
            model.Title,
            model.Bounds.ToDomainBounds(),
            user);

        return new OkResult();
    }

    public record CreateSeatRequestModel(Guid SeatId, string Title, BoundsModel Bounds);

    private class CreateSeatRequestModelModelValidator : AbstractValidator<CreateSeatRequestModel>
    {
        public CreateSeatRequestModelModelValidator()
        {
            RuleFor(x => x.SeatId).NotEmpty();

            RuleFor(x => x.Title).NotEmpty();

            RuleFor(x => x.Bounds).SetValidator(new BoundsModelValidator());
        }
    }
}