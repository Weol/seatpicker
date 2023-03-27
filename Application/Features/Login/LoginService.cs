using Seatpicker.Application.Features.Login.Ports;
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
    private readonly IInviteDiscordUser inviteDiscordUser;
    private readonly IAuthCertificateProvider authCertificateProvider;

    public LoginService(
        IDiscordAccessTokenProvider discordAccessTokenProvider,
        IDiscordLookupUser discordUserLookup,
        IJwtTokenCreator tokenCreator,
        IAuthCertificateProvider authCertificateProvider,
        IInviteDiscordUser inviteDiscordUser)
    {
        this.discordAccessTokenProvider = discordAccessTokenProvider;
        this.discordUserLookup = discordUserLookup;
        this.tokenCreator = tokenCreator;
        this.authCertificateProvider = authCertificateProvider;
        this.inviteDiscordUser = inviteDiscordUser;
    }

    public async Task<string> GetFor(string discordToken)
    {
        var accessToken = await discordAccessTokenProvider.GetFor(discordToken);
        var user = await discordUserLookup.Lookup(accessToken);

        var inviteTask = inviteDiscordUser.Invite(user, accessToken);

        var authCertificate = await authCertificateProvider.Get();

        var claims = GetRolesForUser(user);
        var token = await tokenCreator.CreateFor(user, authCertificate, claims);

        await inviteTask;

        return token;
    }

    private Role[] GetRolesForUser(User user)
    {
        if (user.Id == "376129925780078592") // Weol's user id
            return new[] { Role.Admin, Role.User };

        return new[] { Role.User };
    }
}