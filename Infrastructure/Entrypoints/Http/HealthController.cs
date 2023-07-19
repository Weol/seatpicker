using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Seatpicker.Infrastructure.Entrypoints.Http;

[ApiController]
[Route("[controller]")]
public class HealthController
{
    private readonly HealthCheckService healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        this.healthCheckService = healthCheckService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var report = await healthCheckService.CheckHealthAsync();

        var result = new HealthCheckResult(report.Status);
        return result.Status is HealthStatus.Healthy ? new OkObjectResult(result) : new ObjectResult(result) { StatusCode = 503 };
    }

    public record HealthCheckResult(HealthStatus Status);
}