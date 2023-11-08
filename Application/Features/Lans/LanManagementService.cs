using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Lans;

public interface ILanManagementService
{
    public Task<Guid> Create(string title, byte[] background, User initiator);

    public Task Update(Guid id, string? title, byte[]? background, User initiator);

    public Task Delete(Guid id, User initiator);
}

internal class LanManagementManagementService : ILanManagementService
{
    private readonly IAggregateRepository repository;

    public LanManagementManagementService(IAggregateRepository repository)
    {
        this.repository = repository;
    }

    public async Task<Guid> Create(string title, byte[] background, User initiator)
    {
        await using var transaction = repository.CreateTransaction();
        var id = Guid.NewGuid();

        var lan = new Lan(id, title, background, initiator);

        transaction.Create(lan);
        transaction.Commit();

        return id;
    }

    public async Task Update(Guid id, string? title, byte[]? background, User initiator)
    {
        await using var transaction = repository.CreateTransaction();

        var lan = await transaction.Aggregate<Lan>(id);
        if (lan is null) throw new LanNotFoundException { LanId = id };

        if (title is not null) lan.ChangeTitle(title, initiator);
        if (background is not null) lan.ChangeBackground(background, initiator);

        transaction.Update(lan);
        transaction.Commit();
    }

    public async Task Delete(Guid id, User initiator)
    {
        await using var transaction = repository.CreateTransaction();

        var lan = await transaction.Aggregate<Lan>(id);
        if (lan is null) throw new LanNotFoundException { LanId = id };

        transaction.Update(lan);
        transaction.Archive(lan);

        transaction.Commit();
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
