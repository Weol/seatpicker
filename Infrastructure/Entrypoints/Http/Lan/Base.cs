using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.LanManagement;
using Seatpicker.Infrastructure.Adapters.Database.Queries;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Lan;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Admin")]
public partial class LanController
{
    private readonly ILanManagementService lanManagementService;
    private readonly IValidateModel validateModel;
    private readonly ILoggedInUserAccessor loggedInUserAccessor;
    private readonly LanQueries lanQueries;

    public LanController(ILanManagementService lanManagementService, IValidateModel validateModel, ILoggedInUserAccessor loggedInUserAccessor, LanQueries lanQueries)
    {
        this.lanManagementService = lanManagementService;
        this.validateModel = validateModel;
        this.loggedInUserAccessor = loggedInUserAccessor;
        this.lanQueries = lanQueries;
    }

    private static bool IsSvg(byte[]? svgImage)
    {
        if (svgImage is null) return false;

        var utf8 = new UTF8Encoding();
        var svgString = utf8.GetString(svgImage);

        return Regex.IsMatch(svgString, @"^\n*(<[!?].+>\n*){0,2}\n*<svg.+\/svg>\n*$");
    }
}