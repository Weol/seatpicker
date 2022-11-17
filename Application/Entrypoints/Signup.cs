using System.Net;
using System.Text.Json;
using Application.Middleware;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Seatpicker.Domain;
using Seatpicker.Domain.UserRegistration;

namespace Application.Entrypoints;

public class Signup
{
    private readonly ILogger<Signup> logger;
    private readonly IUserRegistrationService userRegistrationService;
    private readonly IModelDeserializerService modelDeserializer;

    public Signup(ILogger<Signup> logger, IUserRegistrationService userRegistrationService, IModelDeserializerService modelDeserializer)
    {
        this.logger = logger;
        this.userRegistrationService = userRegistrationService;
        this.modelDeserializer = modelDeserializer;
    }

    [Function(nameof(Signup))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request,
        FunctionContext executionContext)
    {
        var model = await modelDeserializer.Deserialize<SignupModel, SignupModelValidator>(request);
        
        var user = new UnregisteredUser(model.Email, model.Nick, model.Name);
        
        await userRegistrationService.Register(user, model.Password);
        
        return request.CreateResponse(HttpStatusCode.OK);
    }

    private record SignupModel(string Email, string Nick, string Name, string Password);

    private class SignupModelValidator : AbstractValidator<SignupModel> {
        public SignupModelValidator()
        {
            RuleFor(x => x.Nick).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().Length(6, 1024);
        }
    }

}