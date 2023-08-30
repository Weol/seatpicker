using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Seat;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Operator")]
public partial class SeatController
{
    private readonly ISeatManagementService seatManagementService;
    private readonly ILoggedInUserAccessor loggedInUserAccessor;
    private readonly IValidateModel validateModel;

    public SeatController(
        IValidateModel validateModel,
        ILoggedInUserAccessor loggedInUserAccessor,
        ISeatManagementService seatManagementService)
    {
        this.validateModel = validateModel;
        this.loggedInUserAccessor = loggedInUserAccessor;
        this.seatManagementService = seatManagementService;
    }

    public record BoundsModel(double X, double Y, double Width, double Height)
    {
        public Bounds ToDomainBounds()
        {
            return new Bounds(X, Y, Width, Height);
        }
    }

    private class BoundsModelValidator : AbstractValidator<BoundsModel>
    {
        public BoundsModelValidator()
        {
            RuleFor(x => x.Width).GreaterThan(0);

            RuleFor(x => x.Height).GreaterThan(0);
        }
    }
}