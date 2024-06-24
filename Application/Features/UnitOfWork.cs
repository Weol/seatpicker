using Marten;
using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Application.Features.Reservation;

namespace Seatpicker.Application.Features;

public class UnitOfWorkFactory(
    IDocumentStore documentStore,
    IAggregateRepository aggregateRepository,
    IDocumentRepository documentRepository)
{
    public async Task<T> Create<T>(string guildId, Func<GuildScopedUnitOfWork, Task<T>> action)
    {
        var services = new ServiceCollection();

        services.AddSingleton<IDocumentSession>(_ => documentStore.LightweightSession(guildId));

        services.AddSingleton<IAggregateTransaction>(provider =>
            aggregateRepository.CreateTransaction(guildId, provider.GetRequiredService<IDocumentSession>()));
        services.AddSingleton<IDocumentReader>(provider => 
            documentRepository.CreateReader(guildId, provider.GetRequiredService<IDocumentSession>()));
        services.AddSingleton<IDocumentTransaction>(provider => 
            documentRepository.CreateTransaction(guildId, provider.GetRequiredService<IDocumentSession>()));

        services.AddSingleton<GuildScopedUnitOfWork>();

        // Reservation feature
        services.AddSingleton<ReservationService>()
            .AddSingleton<ReservationManagementService>()
            .AddSingleton<SeatManagementService>();

        // Lan feature
        services.AddSingleton<LanService>();

        await using var provider = services.BuildServiceProvider();

        var unitOfWork = provider.GetRequiredService<GuildScopedUnitOfWork>();
        await action(unitOfWork);
        var documentSession = provider.GetRequiredService<IDocumentSession>();
        await documentSession.SaveChangesAsync();
    }
}

public class GuildScopedUnitOfWork(
    ReservationService reservationService,
    ReservationManagementService reservationManagementService,
    SeatManagementService seatManagementService,
    LanService lanService
)
{
    public ReservationService Reservation => reservationService;
    public ReservationManagementService ReservationManagement => reservationManagementService;
    public SeatManagementService SeatManagement => seatManagementService;
    public LanService Lan => lanService;
}