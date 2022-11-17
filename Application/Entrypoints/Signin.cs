using System.Net;
using System.Text.Json;
using Application.Authentication;
using Application.Middleware;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Seatpicker.Domain;
using Seatpicker.Domain.UserRegistration;

namespace Application.Entrypoints;

public class Signin
{
    private readonly ILogger<Signin> logger;
    private readonly ILoginTokenService loginTokenService;
    private readonly IModelDeserializerService modelDeserializer;

    public Signin(ILogger<Signin> logger, ILoginTokenService loginTokenService, IModelDeserializerService modelDeserializer)
    {
        this.logger = logger;
        this.loginTokenService = loginTokenService;
        this.modelDeserializer = modelDeserializer;
    }

    [Function(nameof(Signin))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request,
        FunctionContext executionContext)
    {
        var model = await modelDeserializer.Deserialize<SigninModel, SigninModelValidator>(request);
        
        await loginTokenService.GetTokenFor(model.Email, model.Password);
        
        return request.CreateResponse(HttpStatusCode.OK);
    }

    private record SigninModel(string Email, string Password);

    private class SigninModelValidator : AbstractValidator<SigninModel> {
        public SigninModelValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().Length(6, 1024);
        }
    }

}