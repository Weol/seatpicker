using System.Net;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Seatpicker.Application.Features.Login;

namespace Seatpicker.Infrastructure.Entrypoints;

public class GetLoginToken
{
    private readonly ILoginService loginService;
    private readonly IRequestModelDeserializerService requestModelDeserializer;
    private readonly IResponseModelSerializerService responseModelSerializerService;

    public GetLoginToken(IResponseModelSerializerService responseModelSerializerService,
        IRequestModelDeserializerService requestModelDeserializer,
        ILoginService loginService)
    {
        this.requestModelDeserializer = requestModelDeserializer;
        this.loginService = loginService;
        this.responseModelSerializerService = responseModelSerializerService;
    }

    [Function(nameof(GetLoginToken))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "token")]
        HttpRequestData request,
        FunctionContext executionContext)
    {
        var model = await requestModelDeserializer.Deserialize<LoginRequestModel, LoginModelValidator>(request);

        var loginToken = await loginService.GetFor(model.Token);

        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync(await responseModelSerializerService.Serialize(new LoginResponseModel(loginToken)));
        return response;
    }

    private record LoginRequestModel(string Token);

    private record LoginResponseModel(string Token);

    private class LoginModelValidator : AbstractValidator<LoginRequestModel>
    {
        public LoginModelValidator()
        {
            RuleFor(x => x.Token).NotEmpty();
        }
    }
}