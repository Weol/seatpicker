using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Login;
using Seatpicker.Infrastructure.ModelValidation;

namespace Seatpicker.Infrastructure.Entrypoints;

[ApiController]
[Route("token")]
public class LoginTokenController
{
    private readonly ILoginService loginService;
    private readonly IModelValidator modelValidator;

    public LoginTokenController(ILoginService loginService, IModelValidator modelValidator)
    {
        this.loginService = loginService;
        this.modelValidator = modelValidator;
    }

    [HttpPost]
    public async Task<ActionResult<TokenResponseModel>> Create(TokenRequestModel model)
    {
        await modelValidator.Validate<TokenRequestModel, TokenRequestModelValidator>(model);
        
        var token = await loginService.GetFor(model.Token);

        return new OkObjectResult(new TokenResponseModel(token));
    }

    public record TokenRequestModel(string Token);

    public record TokenResponseModel(string Token);

    private class TokenRequestModelValidator : AbstractValidator<TokenRequestModel>
    {
        public TokenRequestModelValidator()
        {
            RuleFor(x => x.Token).NotEmpty();
        }
    }
}