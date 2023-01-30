using System.Net;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Seatpicker.Infrastructure.Entrypoints;

public class PutReservation
{
    private readonly IRequestModelDeserializerService requestModelDeserializer;
    private readonly IResponseModelSerializerService responseModelSerializerService;

    public PutReservation(IResponseModelSerializerService responseModelSerializerService,
        IRequestModelDeserializerService requestModelDeserializer)
    {
        this.requestModelDeserializer = requestModelDeserializer;
        this.responseModelSerializerService = responseModelSerializerService;
    }

    [Function(nameof(PutReservation))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reservations/{reservationid:Guid}")]
        HttpRequestData request,
        FunctionContext executionContext)
    {
        var model = await requestModelDeserializer.Deserialize<LoginRequestModel, LoginModelValidator>(request);

        
        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync(await responseModelSerializerService.Serialize(new LoginResponseModel("asd")));
        return response;
    }

    private record LoginRequestModel(Guid SeatId);

    private record LoginResponseModel(string Token);

    private class LoginModelValidator : AbstractValidator<LoginRequestModel>
    {
        public LoginModelValidator()
        {
        }
    }
}