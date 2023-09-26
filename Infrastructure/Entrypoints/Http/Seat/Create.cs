using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

public partial class SeatController
{
    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Create([FromBody] CreateSeatRequest model)
    {
        await validateModel.Validate<CreateSeatRequest, CreateSeatRequestModelValidator>(model);

        var user = loggedInUserAccessor.Get();

        var seatId = await seatManagementService.Create(
            model.Title,
            model.Bounds.ToDomainBounds(),
            user);

        return new CreatedResult(seatId.ToString(), null);
    }

    public record CreateSeatRequest(string Title, BoundsModel Bounds);

    private class CreateSeatRequestModelValidator : AbstractValidator<CreateSeatRequest>
    {
        public CreateSeatRequestModelValidator()
        {
            RuleFor(x => x.Title).NotEmpty();

            RuleFor(x => x.Bounds).SetValidator(new BoundsModelValidator());
        }
    }
}