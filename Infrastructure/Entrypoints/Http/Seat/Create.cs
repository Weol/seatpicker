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

        var seatId = await seatManagementService.Create(
            model.Title,
            model.Bounds.ToDomainBounds(),
            user);

        return new CreatedResult(seatId.ToString(), null);
    }

    public record CreateSeatRequestModel(string Title, BoundsModel Bounds);

    private class CreateSeatRequestModelModelValidator : AbstractValidator<CreateSeatRequestModel>
    {
        public CreateSeatRequestModelModelValidator()
        {
            RuleFor(x => x.Title).NotEmpty();

            RuleFor(x => x.Bounds).SetValidator(new BoundsModelValidator());
        }
    }
}