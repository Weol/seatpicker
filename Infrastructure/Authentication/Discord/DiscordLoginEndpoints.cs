using System.Diagnostics.CodeAnalysis;
using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Domain;
using Seatpicker.Infrastructure.Adapters.DiscordClient;
using Seatpicker.Infrastructure.Entrypoints.Http;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

namespace Seatpicker.Infrastructure.Authentication.Discord;

public static class DiscordLoginEndpoints
{
    public static async Task<IResult> Renew(
        [FromBody] TokenRenewModel model,
        IValidateModel validateModel,
        DiscordClient discordClient,
        DiscordJwtTokenCreator tokenCreator,
        HttpContext httpContext)
    {
        try
        {
            await validateModel.Validate<TokenRenewModel, TokenRenewModelValidator>(model);

            var accessToken = await discordClient.RefreshAccessToken(model.RefreshToken);
            var discordUser = await discordClient.Lookup(accessToken.AccessToken);

            return Results.Ok(await CreateTokenRequestModel(accessToken, discordUser, tokenCreator));
        }
        catch (DiscordException e) when (e.StatusCode == HttpStatusCode.BadRequest)
        {
            return Results.BadRequest("Token is invalid");
        }
        catch (ModelValidationException e)
        {
            return Results.BadRequest(e.ValidationResultErrors);
        }
    }

    public static async Task<IResult> Login(
        [FromBody] TokenRequestModel model,
        IValidateModel validateModel,
        DiscordClient discordClient,
        DiscordJwtTokenCreator tokenCreator)
    {
        try
        {
            await validateModel.Validate<TokenRequestModel, TokenRequestModelValidator>(model);

            var accessToken = await discordClient.GetAccessToken(model.Token);
            var discordUser = await discordClient.Lookup(accessToken.AccessToken);

            return Results.Ok(await CreateTokenRequestModel(accessToken, discordUser, tokenCreator));
        }
        catch (DiscordException e) when (e.StatusCode == HttpStatusCode.BadRequest)
        {
            return Results.BadRequest("Token is invalid");
        }
        catch (ModelValidationException e)
        {
            return Results.BadRequest(e.ValidationResultErrors);
        }
    }

    private static async Task<TokenResponseModel> CreateTokenRequestModel(
        DiscordAccessToken accessToken,
        DiscordUser discordUser,
        DiscordJwtTokenCreator tokenCreator)
    {
        // Minus 10 just to make sure that the discord token expires a bit after the jwt token actually expires
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(accessToken.ExpiresIn - 10);

        var token = new DiscordToken(
            discordUser.Id,
            discordUser.Username,
            discordUser.Avatar,
            accessToken.RefreshToken,
            expiresAt);

        var jwtToken = await tokenCreator.CreateToken(token, GetRolesForUser(discordUser));

        return new TokenResponseModel(jwtToken);
    }

    private static Role[] GetRolesForUser(DiscordUser discordUser)
    {
        if (discordUser.Id == "376129925780078592") // Weol's user id
            return new[] { Role.Admin, Role.Operator, Role.User };

        return new[] { Role.User };
    }

    public record TokenRequestModel(string Token);

    public record TokenRenewModel(string RefreshToken);

    public class TokenRequestModelValidator : AbstractValidator<TokenRequestModel>
    {
        public TokenRequestModelValidator()
        {
            RuleFor(x => x.Token).NotEmpty();
        }
    }

    public class TokenRenewModelValidator : AbstractValidator<TokenRenewModel>
    {
        public TokenRenewModelValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }

    public record TokenResponseModel(string Token);
}