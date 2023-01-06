using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Seatpicker.UserContext.Application.UserToken;
using Seatpicker.UserContext.Application.UserToken.Ports;
using Seatpicker.UserContext.Domain.Registration.Ports;

namespace Seatpicker.UserContext.Domain.Registration;

public interface ILoginService
{
    public Task<string> GetFor(string discordToken);
}

internal class LoginService : ILoginService
{
    public IAuthCertificateProvider CertificateProvider { get; }

    private static readonly Role[] DefaultRoles =
    {
        Role.SeatReserver,
        Role.ReservedSeatsViewer
    };

    private readonly ILogger<LoginService> logger;
    private readonly IStoreUser storeUser;
    private readonly IUserTokenService userTokenService;
    private readonly ILookupUser lookupUser;
    private readonly IDiscordAccessTokenProvider discordAccessTokenProvider;
    private readonly IDiscordLookupUser discordUserLookup;

    public LoginService(ILogger<LoginService> logger,
        IAuthCertificateProvider certificateProvider, IStoreUser storeUser, ILookupUser lookupUser,
        IDiscordAccessTokenProvider discordAccessTokenProvider, IDiscordLookupUser discordUserLookup, IUserTokenService userTokenService)
    {
        CertificateProvider = certificateProvider;
        this.logger = logger;
        this.storeUser = storeUser;
        this.lookupUser = lookupUser;
        this.discordAccessTokenProvider = discordAccessTokenProvider;
        this.discordUserLookup = discordUserLookup;
        this.userTokenService = userTokenService;
    }

    public async Task<string> GetFor(string discordToken)
    {
        var accessToken = await discordAccessTokenProvider.GetFor(discordToken);
        var discordUser = await discordUserLookup.Lookup(accessToken.AccessToken);

        var user = await lookupUser.Lookup(discordUser.Id);

        if (user is not null)
        {
            user = new User(discordUser.Id, discordUser.Username, discordUser.Avatar, user.Roles, DateTime.Now);
        }
        else
        {
            user = new User(discordUser.Id, discordUser.Username, discordUser.Avatar, DefaultRoles, DateTime.Now);
        }

        await storeUser.Store(user);
        
        var token = await userTokenService.GetJwtFor(user);

        return token;
    }
}

internal static class LoginServiceExtensions
{
    public static IServiceCollection AddLoginService(this IServiceCollection services)
    {
        return services.AddScoped<ILoginService, LoginService>();
    }
}