using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Lans;

public interface ILanManagementService
{
    public Task<Guid> Create(string title, string guildId, byte[] background, User initiator);

    public Task Update(Guid id, bool? active, string? title, byte[]? background, User initiator);

    public Task Delete(Guid id, User initiator);
}

internal class LanManagementManagementService : ILanManagementService
{
    private readonly IAggregateRepository aggregateRepository;
    private readonly IDocumentRepository documentRepository;

    public LanManagementManagementService(IAggregateRepository aggregateRepository, IDocumentRepository documentRepository)
    {
        this.aggregateRepository = aggregateRepository;
        this.documentRepository = documentRepository;
    }

    public async Task<Guid> Create(string title, string guildId, byte[] background, User initiator)
    {
        using var transaction = aggregateRepository.CreateTransaction();
        var id = Guid.NewGuid();

        var lan = new Lan(id, title, background, guildId, initiator);

        transaction.Create(lan);
        await transaction.Commit();

        return id;
    }

    public async Task Update(Guid id, bool? active, string? title, byte[]? background, User initiator)
    {
        using var transaction = aggregateRepository.CreateTransaction();

        var lan = await transaction.Aggregate<Lan>(id);
        if (lan is null) throw new LanNotFoundException { LanId = id };

        if (title is not null) lan.ChangeTitle(title, initiator);
        if (background is not null) lan.ChangeBackground(background, initiator);
        if (active is not null) await SetActive(transaction, lan, active.Value, initiator);

        transaction.Update(lan);
        await transaction.Commit();
    }

    public async Task Delete(Guid id, User initiator)
    {
        using var transaction = aggregateRepository.CreateTransaction();

        var lan = await transaction.Aggregate<Lan>(id);
        if (lan is null) throw new LanNotFoundException { LanId = id };

        lan.Archive(initiator);

        transaction.Update(lan);
        transaction.Archive(lan);

        await transaction.Commit();
    }

    private async Task SetActive(IAggregateTransaction transaction, Lan lan, bool active, User initiator)
    {
        using var reader = documentRepository.CreateReader();

        var activeLans = reader.Query<ProjectedLan>()
            .Where(x => x.GuildId == lan.GuildId)
            .Where(x => x.Active)
            .Select(x => x.Id)
            .ToArray();

        foreach (var activeLanId in activeLans)
        {
            var activeLan = await transaction.Aggregate<Lan>(activeLanId)
                ?? throw new LanNotFoundException { LanId = activeLanId };

            activeLan.SetActive(false, initiator);
            transaction.Update(activeLan);
        }

        lan.SetActive(active, initiator);

        transaction.Update(lan);
    }
}

/**
 * Exceptions
 */
public class LanNotFoundException : ApplicationException
{
    public required Guid LanId { get; init; }

    protected override string ErrorMessage => $"Lan with id {LanId} was not found";
}