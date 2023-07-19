using System.Net;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Controllers;

[ApiController]
[Route("[controller]")]
public class SeatController
{
    private readonly ILoggedInUserAccessor loggedInUserAccessor;

    public SeatController(ISeatRepository seatRepository, ILoggedInUserAccessor loggedInUserAccessor)
    {
        this.loggedInUserAccessor = loggedInUserAccessor;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Seat>), (int) HttpStatusCode.OK)]
    public async Task<IActionResult> Get()
    {
        return new OkResult();
    }
}