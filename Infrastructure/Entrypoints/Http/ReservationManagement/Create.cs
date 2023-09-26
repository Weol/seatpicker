using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;

public partial class ReservationManagementController
{
    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Create([FromBody] CreateReservationForRequest model)
    {
        await validateModel.Validate<CreateReservationForRequest, CreateReservationForRequestValidator>(model);

        var user = loggedInUserAccessor.Get();

        await reservationManagementService.Create(model.SeatId, new UserId(model.UserId), user);

        return new OkResult();
    }

    public record CreateReservationForRequest(Guid SeatId, string UserId);

    private class CreateReservationForRequestValidator : AbstractValidator<CreateReservationForRequest>
    {
        public CreateReservationForRequestValidator()
        {
            RuleFor(x => x.SeatId)
                .NotEmpty();

            RuleFor(x => x.UserId)
                .NotEmpty();
        }
    }
}