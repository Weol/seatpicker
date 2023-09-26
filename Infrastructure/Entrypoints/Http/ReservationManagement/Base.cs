using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Seats;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.ReservationManagement;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Operator")]
public partial class ReservationManagementController
{
    private readonly IReservationManagementService reservationManagementService;
    private readonly ILoggedInUserAccessor loggedInUserAccessor;
    private readonly IValidateModel validateModel;

    public ReservationManagementController(
        IReservationManagementService reservationManagementService,
        IValidateModel validateModel,
        ILoggedInUserAccessor loggedInUserAccessor)
    {
        this.reservationManagementService = reservationManagementService;
        this.validateModel = validateModel;
        this.loggedInUserAccessor = loggedInUserAccessor;
    }
}