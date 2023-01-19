using Microsoft.Extensions.DependencyInjection;
using Seatpicker.SeatContext.Registration.Ports;

namespace Seatpicker.SeatContext.Registration;

public interface ILoginService
{
    public Task<string> GetFor(string discordToken);
}

internal class LoginService : ILoginService
{
    private static readonly Role[] DefaultRoles =
    {
        Role.SeatReserver,
        Role.ReservedSeatsViewer
    };

    private readonly ILogger<LoginService> logger;
    private readonly IStoreUser storeUser;
    private readonly ICreateJwtToken createJwtToken;
    private readonly ILookupUser lookupUser;
    private readonly IDiscordAccessTokenProvider discordAccessTokenProvider;
    private readonly IDiscordLookupUser discordUserLookup;

    public LoginService(ILogger<LoginService> logger,
        IDiscordAccessTokenProvider discordAccessTokenProvider, 
        IDiscordLookupUser discordUserLookup,
        ICreateJwtToken createJwtToken, 
        IStoreUser storeUser, 
        ILookupUser lookupUser)
    {
        this.logger = logger;
        this.storeUser = storeUser;
        this.lookupUser = lookupUser;
        this.discordAccessTokenProvider = discordAccessTokenProvider;
        this.discordUserLookup = discordUserLookup;
        this.createJwtToken = createJwtToken;
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

        var token = await createJwtToken.ForUser(user);

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