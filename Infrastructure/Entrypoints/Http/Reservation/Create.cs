using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Reservation;

public partial class ReservationController
{
    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Create([FromBody] CreateReservationRequest model)
    {
        await validateModel.Validate<CreateReservationRequest, CreateReservationRequestValidator>(model);

        var user = loggedInUserAccessor.Get();

        await reservationService.Create(model.SeatId, user);

        return new OkResult();
    }

    public record CreateReservationRequest(Guid SeatId);

    private class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
    {
        public CreateReservationRequestValidator()
        {
            RuleFor(x => x.SeatId)
                .NotEmpty();
        }
    }
}