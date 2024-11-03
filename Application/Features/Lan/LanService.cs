using Seatpicker.Domain;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace Seatpicker.Application.Features.Lan;

public class LanService(IAggregateTransaction aggregateTransaction, IDocumentReader documentReader, User actor)
{
    public async Task<string> Create(string title, byte[] background)
    {
        var id = Guid.NewGuid().ToString();
        var lan = new Domain.Lan(id, title, background, actor);

        aggregateTransaction.Create(lan);

        return id;
    }

    public async Task UpdateBackground(string id, byte[] background)
    {
        var lan = await aggregateTransaction.Aggregate<Domain.Lan>(id);
        if (lan is null) throw new LanNotFoundException { LanId = id };

        lan.ChangeBackground(background, actor);

        aggregateTransaction.Update(lan);
    }

    public async Task UpdateTitle(string id, string title)
    {
        var lan = await aggregateTransaction.Aggregate<Domain.Lan>(id);
        if (lan is null) throw new LanNotFoundException { LanId = id };

        lan.ChangeTitle(title, actor);

        aggregateTransaction.Update(lan);
    }

    public async Task SetActive(string id, bool active)
    {
        var lan = await aggregateTransaction.Aggregate<Domain.Lan>(id);
        if (lan is null) throw new LanNotFoundException { LanId = id };

        var activeLans = documentReader.Query<ProjectedLan>()
            .Where(x => x.Active)
            .Select(x => x.Id)
            .ToArray();

        foreach (var activeLanId in activeLans)
        {
            var activeLan = await aggregateTransaction.Aggregate<Domain.Lan>(activeLanId) ??
                            throw new LanNotFoundException { LanId = activeLanId };

            activeLan.SetActive(false, actor);
            aggregateTransaction.Update(activeLan);
        }

        lan.SetActive(false, actor);

        aggregateTransaction.Update(lan);
    }

    public async Task Delete(string id)
    {
        var lan = await aggregateTransaction.Aggregate<Domain.Lan>(id);
        if (lan is null) throw new LanNotFoundException { LanId = id };

        lan.Archive(actor);

        aggregateTransaction.Update(lan);
        aggregateTransaction.Archive(lan);
    }
}

/**
 * Exceptions
 */
public class LanNotFoundException : ApplicationException
{
    public required string LanId { get; init; }

    protected override string ErrorMessage => $"Lan with id {LanId} was not found";
}

public class GuildNotFoundException : ApplicationException
{
    public required string GuildId { get; init; }

    protected override string ErrorMessage => $"Guild with id {GuildId} was not found";
}