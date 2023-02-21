namespace Seatpicker.Infrastructure.Adapters.Database;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        return services.AddDbContext<SeatpickerContext>();
    }
}