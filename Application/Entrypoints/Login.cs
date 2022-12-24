using System.Net;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Seatpicker.Domain.Domain.Registration;

namespace Application.Entrypoints;

public class Login
{
    private readonly ILogger<Login> logger;
    private readonly ILoginService loginService;
    private readonly IRequestModelDeserializerService requestModelDeserializer;
    private readonly IResponseModelSerializerService responseModelSerializerService;

    public Login(ILogger<Login> logger, IRequestModelDeserializerService requestModelDeserializer, ILoginService loginService, IResponseModelSerializerService responseModelSerializerService)
    {
        this.logger = logger;
        this.requestModelDeserializer = requestModelDeserializer;
        this.loginService = loginService;
        this.responseModelSerializerService = responseModelSerializerService;
    }

    [Function(nameof(Login))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request,
        FunctionContext executionContext)
    {
        var model = await requestModelDeserializer.Deserialize<LoginRequestModel, LoginModelValidator>(request);

        var loginToken = await loginService.GetFor(model.Token);

        return await responseModelSerializerService.Serialize(
            request.CreateResponse(HttpStatusCode.OK),
            new LoginResponseModel(loginToken));
    }

    private record LoginRequestModel(string Token);
    
    private record LoginResponseModel(string Token);

    private class LoginModelValidator : AbstractValidator<LoginRequestModel> {
        public LoginModelValidator()
        {
            RuleFor(x => x.Token).NotEmpty();
        }
    }

}