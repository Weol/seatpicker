using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features.Token;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Entrypoints.Http;

[ApiController]
[Route("token")]
public class TokenController
{
    private readonly ILoginService loginService;
    private readonly IValidateModel validateModel;

    public TokenController(ILoginService loginService, IValidateModel validateModel)
    {
        this.loginService = loginService;
        this.validateModel = validateModel;
    }

    [HttpPost]
    public async Task<ActionResult<TokenResponseModel>> Create(TokenRequestModel model)
    {
        await validateModel.Validate<TokenRequestModel, TokenRequestModelValidator>(model);

        var token = await loginService.GetFor(model.Token);

        return new OkObjectResult(new TokenResponseModel(token));
    }

    [HttpGet("test")]
    [Authorize]
    public Task<ActionResult> Test()
    {
        return Task.FromResult<ActionResult>(new OkResult());
    }

    public record TokenRequestModel(string Token);

    public record TokenResponseModel(string Token);

    private class TokenRequestModelValidator : AbstractValidator<TokenRequestModel>
    {
        public TokenRequestModelValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty();
        }
    }
}