using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Reservation;

[ApiController]
[Route("[controller]")]
[Authorize]
public partial class ReservationController
{
    private readonly IReservationService reservationService;
    private readonly ILoggedInUserAccessor loggedInUserAccessor;
    private readonly IValidateModel validateModel;

    public ReservationController(
        IReservationService reservationService,
        IValidateModel validateModel,
        ILoggedInUserAccessor loggedInUserAccessor)
    {
        this.reservationService = reservationService;
        this.validateModel = validateModel;
        this.loggedInUserAccessor = loggedInUserAccessor;
    }
}