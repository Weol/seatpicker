using Seatpicker.Application.Features;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Adapters;

internal class SeatRepository : ISeatRepository
{
    public Task<ICollection<Seat>> GetAll() => throw new NotImplementedException();

    public Task<Seat?> Get(Guid seatId) => throw new NotImplementedException();

    public Task<Seat?> GetByUserId(string userId) => throw new NotImplementedException();
    public Task Store(Seat seat) => throw new NotImplementedException();
}

internal static class SeatRepositoryExtensions
{
    internal static IServiceCollection AddSeatRepository(this IServiceCollection services)
    {
        services.AddSingleton<ISeatRepository, SeatRepository>();

        return services;
    }
}
