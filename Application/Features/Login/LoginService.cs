﻿using Seatpicker.Application.Features.Login.Ports;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Login;

public interface ILoginService
{
    public Task<string> GetFor(string discordToken);
}

internal class LoginService : ILoginService
{
    private readonly IJwtTokenCreator tokenCreator;
    private readonly IDiscordAccessTokenProvider discordAccessTokenProvider;
    private readonly IDiscordLookupUser discordUserLookup;
    private readonly IAuthCertificateProvider authCertificateProvider;

    public LoginService(
        IDiscordAccessTokenProvider discordAccessTokenProvider,
        IDiscordLookupUser discordUserLookup,
        IJwtTokenCreator tokenCreator,
        IAuthCertificateProvider authCertificateProvider)
    {
        this.discordAccessTokenProvider = discordAccessTokenProvider;
        this.discordUserLookup = discordUserLookup;
        this.tokenCreator = tokenCreator;
        this.authCertificateProvider = authCertificateProvider;
    }

    public async Task<string> GetFor(string discordToken)
    {
        var accessToken = await discordAccessTokenProvider.GetFor(discordToken);
        var discordUser = await discordUserLookup.Lookup(accessToken);

        var user = new User{
            Id = discordUser.Id,
            Nick = discordUser.Username,
            Avatar = discordUser.Avatar,
        };

        var authCertificate = await authCertificateProvider.Get();

        var claims = GetRolesForUser(user);
        var token = await tokenCreator.CreateFor(user, authCertificate, claims);

        return token;
    }

    private Role[] GetRolesForUser(User user)
    {
        if (user.Id == "376129925780078592") // Weol's user id
            return new[] { Role.Admin, Role.User };

        return new[] { Role.User };
    }
}