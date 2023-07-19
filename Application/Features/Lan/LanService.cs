using Marten;

namespace Seatpicker.Application.Features.Lan;

public interface ILanService
{
    public Task Create(CreateLan createLan);

    public Task Update(UpdateLan updateLan);

    public Task<Domain.Lan> Get(Guid lanId);
}

internal class LanService : ILanService
{
    private readonly IDocumentStore store;

    public LanService(IDocumentStore store)
    {
        this.store = store;
    }

    public async Task Create(CreateLan createLan)
    {
        await using var session = store.LightweightSession();

        var lan = session.Events.AggregateStream<Domain.Lan>(createLan.Id);
        if (lan is not null) throw new LanAlreadyExistsException { LanId = lan.Id };

        lan = new Domain.Lan(createLan.Id, createLan.Title, createLan.Background);

        session.Events.StartStream<Domain.Lan>(lan.Id, lan.RaisedEvents);

        await session.SaveChangesAsync();
    }

    public async Task Update(UpdateLan updateLan)
    {
        await using var session = store.LightweightSession();

        var lan = session.Events.AggregateStream<Domain.Lan>(updateLan.Id);
        if (lan is null) throw new LanNotFoundException { LanId = updateLan.Id };

        if (updateLan.Title is not null) lan.ChangeTitle(updateLan.Title);
        if (updateLan.Background is not null) lan.ChangeBackground(updateLan.Background);

        session.Events.Append(lan.Id, lan.RaisedEvents);
        await session.SaveChangesAsync();
    }

    public async Task<Domain.Lan> Get(Guid lanId)
    {
        await using var session = store.LightweightSession();

        var lan = session.Events.AggregateStream<Domain.Lan>(lanId);
        if (lan is null) throw new LanNotFoundException { LanId = lanId };

        return lan;
    }
}

public record CreateLan(Guid Id, string Title, byte[] Background);

public record UpdateLan(Guid Id, string? Title, byte[]? Background);

/**
 * Exceptions
 */
public class LanAlreadyExistsException : DomainException
{
    public required Guid LanId { get; init; }

    public override string Message => $"Lan with id {LanId} already exists";
}

public class LanNotFoundException : DomainException
{
    public required Guid LanId { get; init; }

    public override string Message => $"Lan with id {LanId} was not found";
}
