using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Management.Lan;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Admin")]
public partial class LanController
{
    private readonly ILanManagementService lanManagementService;
    private readonly IValidateModel validateModel;

    public LanController(ILanManagementService lanManagementService, IValidateModel validateModel)
    {
        this.lanManagementService = lanManagementService;
        this.validateModel = validateModel;
    }

    private static bool IsSvg(byte[]? svgImage)
    {
        if (svgImage is null) return false;

        var utf8 = new UTF8Encoding();
        var svgString = utf8.GetString(svgImage);

        return Regex.IsMatch(svgString, @"^\n*(<[!?].+>\n*){0,2}\n*<svg.+\/svg>\n*$");
    }
}