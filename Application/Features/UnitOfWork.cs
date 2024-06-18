using Marten;
using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Application.Features.Lans;
using Seatpicker.Application.Features.Seats;

namespace Seatpicker.Application.Features;

public class UnitOfWorkFactory(IDocumentStore documentStore, IAggregateRepository aggregateRepository, IDocumentRepository documentRepository)
{
    public UnitOfWork Create(string guildId, Action<UnitOfWork> action)
    {
        var services = new ServiceCollection();

        var querySession = documentStore.QuerySession(guildId);
        var writeSession = documentStore.LightweightSession(guildId);

        using var aggregateTransaction = writeSession.Events
        using var documentTransaction = documentRepository.CreateTransaction(guildId);
        using var documentReader = documentRepository.CreateReader(guildId);

        services.AddSingleton<GuildDependenBuilder<T>>()

        var provider = services.BuildServiceProvider();




        var unitOfWork = new UnitOfWork
        {
            LanManagement = new LanManagementService(),
            SeatManagement = new SeatManagementService(),
            ReservationManagement = new ReservationManagementService(),
            Reservation = new ReservationService()
        };

        action();
    }
}

public class GuildDependenBuilder<T>
    where T : GuildDependent;

public interface GuildDependent;

public interface IPorta : GuildDependent

public class UnitOfWork(IServiceProvider provider, IPorta porta)
{

}