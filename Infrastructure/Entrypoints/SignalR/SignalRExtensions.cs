using Seatpicker.Application.Features.Reservation;

namespace Seatpicker.Infrastructure.Entrypoints.SignalR;

public static class SignalRExtensions
{
    public static IServiceCollection AddSignalREntrypoints(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddScoped<IReservationNotifier, ReservationNotifier>();

        return services;
    }
    
    public static WebApplication UseSignalREntrypoints(this WebApplication app)
    {
        app.MapHub<ReservationHub>("hubs/reservation");
        
        return app;
    }
}