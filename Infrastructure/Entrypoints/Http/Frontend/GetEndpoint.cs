using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Authentication.Discord.DiscordClient;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

[ApiController]
[Route("applicationinsights")]
[Area("frontend")]
public class ApplicationInsightsConnectionStringEndpoint
{
    [HttpGet("")]
    public async Task<ActionResult<Response[]>> Get(
        [FromServices] IConfiguration configuration)
    {
        var connectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

        if (connectionString is null) return new NotFoundObjectResult("Connection string not found");
        
        return new OkObjectResult(new Response(connectionString));
    }
    
    public record Response(string ConnectionString);
}