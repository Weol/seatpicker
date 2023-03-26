using Seatpicker.Application.Features.Reservation.EventHandlers;

namespace Seatpicker.Infrastructure.Adapters;

public static class DiscordClientExtensions
{
    public static IServiceCollection AddDiscordClient(this IServiceCollection services, Action<DiscordClientOptions> configureAction)
    {
        services.Configure(configureAction);

        return services
            .AddSingleton<DiscordClient>()
            .AddSingleton<IInviteDiscordUser>(provider => provider.GetRequiredService<DiscordClient>());
    }
}
