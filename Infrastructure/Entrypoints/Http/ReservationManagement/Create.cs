using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;

public partial class ReservationManagementController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationForRequestModel model)
    {
        await validateModel.Validate<CreateReservationForRequestModel, CreateReservationForRequestModelValidator>(model);

        var user = loggedInUserAccessor.Get();

        await reservationManagementService.Create(model.SeatId, new UserId(model.UserId), user);

        return new OkResult();
    }

    public record CreateReservationForRequestModel(Guid SeatId, string UserId);

    private class CreateReservationForRequestModelValidator : AbstractValidator<CreateReservationForRequestModel>
    {
        public CreateReservationForRequestModelValidator()
        {
            RuleFor(x => x.SeatId)
                .NotEmpty();

            RuleFor(x => x.UserId)
                .NotEmpty();
        }
    }
}