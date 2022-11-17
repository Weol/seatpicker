using System.Net;
using Application.Authentication;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Application.Entrypoints;

public class Login
{
    private readonly ILogger<Login> logger;
    private readonly ILoginJwtService loginJwtService;
    private readonly IModelDeserializerService modelDeserializer;

    public Login(ILogger<Login> logger, ILoginJwtService loginJwtService, IModelDeserializerService modelDeserializer)
    {
        this.logger = logger;
        this.loginJwtService = loginJwtService;
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