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
    private readonly JsonSerializerOptions serializerOptions;
    private readonly SignupModelValidator validator = new();

    public Signup(ILogger<Signup> logger, IUserRegistrationService userRegistrationService, JsonSerializerOptions serializerOptions)
    {
        this.logger = logger;
        this.userRegistrationService = userRegistrationService;
        this.serializerOptions = serializerOptions;
    }

    [Function(nameof(Signup))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request,
        FunctionContext executionContext)
    {
        var body = await request.ReadAsStringAsync() ?? throw new NullReferenceException();
        var model = JsonSerializer.Deserialize<SignupModel>(body, serializerOptions) ?? throw new NullReferenceException();
        
        ValidateModel(model);
        
        var user = new UnregisteredUser(model.EmailId, model.Nick, model.Name);
        
        await userRegistrationService.Register(user, model.Password);
        
        return request.CreateResponse(HttpStatusCode.OK);
    }

    private void ValidateModel(SignupModel model)
    {
        var result = validator.Validate(model);
        if (!result.IsValid) throw new ModelValidationException(result);
    }

    private record SignupModel(string EmailId, string Nick, string Name, string Password);

    private class SignupModelValidator : AbstractValidator<SignupModel> {
        public SignupModelValidator()
        {
            RuleFor(x => x.Nick).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.EmailId).NotEmpty().Length(64).Matches("[A-Fa-f\\d]+");
            RuleFor(x => x.Password).NotEmpty().Length(64).Matches("[A-Fa-f\\d]+");
        }
    }

}