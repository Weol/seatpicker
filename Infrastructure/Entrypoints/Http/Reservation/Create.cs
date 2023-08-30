using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Reservation;

public partial class ReservationController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationRequestModel model)
    {
        await validateModel.Validate<CreateReservationRequestModel, CreateReservationRequestModelValidator>(model);

        var user = loggedInUserAccessor.Get();

        await reservationService.Create(model.SeatId, user);

        return new OkResult();
    }

    public record CreateReservationRequestModel(Guid SeatId);

    private class CreateReservationRequestModelValidator : AbstractValidator<CreateReservationRequestModel>
    {
        public CreateReservationRequestModelValidator()
        {
            RuleFor(x => x.SeatId)
                .NotEmpty();
        }
    }
}