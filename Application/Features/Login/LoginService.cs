using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Seatpicker.Application.Features.Login.Ports;
using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Login;

public interface ILoginService
{
    public Task<string> GetFor(string discordToken);
}

internal class LoginService : ILoginService
{
    private readonly ILogger<LoginService> logger;
    private readonly IJwtTokenService createJwtToken;
    private readonly IDiscordAccessTokenProvider discordAccessTokenProvider;
    private readonly IDiscordLookupUser discordUserLookup;
    private readonly IAuthCertificateProvider authCertificateProvider;

    public LoginService(ILogger<LoginService> logger,
        IDiscordAccessTokenProvider discordAccessTokenProvider,
        IDiscordLookupUser discordUserLookup,
        IJwtTokenService createJwtToken,
        IAuthCertificateProvider authCertificateProvider)
    {
        this.logger = logger;
        this.discordAccessTokenProvider = discordAccessTokenProvider;
        this.discordUserLookup = discordUserLookup;
        this.createJwtToken = createJwtToken;
        this.authCertificateProvider = authCertificateProvider;
    }

    public async Task<string> GetFor(string discordToken)
    {
        var accessToken = await discordAccessTokenProvider.GetFor(discordToken);
        var discordUser = await discordUserLookup.Lookup(accessToken);

        var user = new User(discordUser.Id, discordUser.Username, discordUser.Avatar);

        var authCertificate = await authCertificateProvider.Get();
        var token = await createJwtToken.CreateFor(user, authCertificate);

        return token;
    }
}