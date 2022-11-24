using System.Net;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Seatpicker.Domain.Application.UserToken;

namespace Application.Entrypoints;

public class Login
{
    private readonly ILogger<Login> logger;
    private readonly IUserTokenService userTokenService;
    private readonly IModelDeserializerService modelDeserializer;

    public Login(ILogger<Login> logger, IUserTokenService userTokenService, IModelDeserializerService modelDeserializer)
    {
        this.logger = logger;
        this.userTokenService = userTokenService;
        this.modelDeserializer = modelDeserializer;
    }

    [Function(nameof(Login))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request,
        FunctionContext executionContext)
    {
        var model = await modelDeserializer.Deserialize<LoginModel, LoginModelValidator>(request);
        
        
        
        return request.CreateResponse(HttpStatusCode.OK);
    }

    private record LoginModel(string Token);

    private class LoginModelValidator : AbstractValidator<LoginModel> {
        public LoginModelValidator()
        {
            RuleFor(x => x.Token).NotEmpty();
        }
    }

}