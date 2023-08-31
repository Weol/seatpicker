using Seatpicker.Domain;

namespace Seatpicker.Application.Features.LanManagement;

public interface ILanManagementService
{
    public Task<Guid> Create(string title, byte[] background);

    public Task Update(Guid id, string? title, byte[]? background);

    public Task<Lan> Get(Guid lanId);
}

internal class LanManagementManagementService : ILanManagementService
{
    private readonly IAggregateTransaction transaction;

    public LanManagementManagementService(IAggregateTransaction transaction)
    {
        this.transaction = transaction;
    }

    public async Task<Guid> Create(string title, byte[] background)
    {
        var id = Guid.NewGuid();

        var lan = new Lan(id, title, background);

        transaction.Create(lan);

        return id;
    }

    public async Task Update(Guid id, string? title, byte[]? background)
    {
        var lan = await transaction.Aggregate<Lan>(id);
        if (lan is null) throw new LanNotFoundException { LanId = id };

        if (title is not null) lan.ChangeTitle(title);
        if (background is not null) lan.ChangeBackground(background);

        transaction.Update(lan);
    }

    public async Task<Lan> Get(Guid lanId)
    {
        var lan = await transaction.Aggregate<Lan>(lanId);
        if (lan is null) throw new LanNotFoundException { LanId = lanId };

        return lan;
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
