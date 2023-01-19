using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Seatpicker.Host.Entrypoints.Seats;

public class GetSeats
{
    private readonly IResponseModelSerializerService responseModelSerializerService;
    private readonly ISeatService seatService;

    public GetSeats(IResponseModelSerializerService responseModelSerializerService, ISeatService seatService)
    {
        this.responseModelSerializerService = responseModelSerializerService;
        this.seatService = seatService;
    }

    [Function(nameof(GetSeats))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "seats")] HttpRequestData request,
        FunctionContext executionContext)
    {
        var seats = await seatService.GetAll();
        
        return await responseModelSerializerService.Serialize(
            request.CreateResponse(HttpStatusCode.OK),
            seats);
    }
}