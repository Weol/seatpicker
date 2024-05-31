using Seatpicker.Application.Features.Seats;

namespace Seatpicker.Infrastructure.Adapters.SignalR;

public static class SignalRExtensions
{
    public static IServiceCollection AddSignalRAdapter(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddScoped<IReservationNotifier, ReservationNotifier>();

        return services;
    }
    
    public static WebApplication UseSignalRAdapter(this WebApplication app)
    {
        app.MapHub<ReservationHub>("hubs/reservation");
         
        return app;
    }
}