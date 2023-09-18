using Seatpicker.Domain;

namespace Seatpicker.Application.Features.LanManagement;

public interface ILanManagementService
{
    public Task<Guid> Create(string title, byte[] background, User initiator);

    public Task Update(Guid id, string? title, byte[]? background, User initiator);
}

internal class LanManagementManagementService : ILanManagementService
{
    private readonly IAggregateTransaction transaction;

    public LanManagementManagementService(IAggregateTransaction transaction)
    {
        this.transaction = transaction;
    }

    public async Task<Guid> Create(string title, byte[] background, User initiator)
    {
        var id = Guid.NewGuid();

        var lan = new Lan(id, title, background, initiator);

        transaction.Create(lan);

        return id;
    }

    public async Task Update(Guid id, string? title, byte[]? background, User initiator)
    {
        var lan = await transaction.Aggregate<Lan>(id);
        if (lan is null) throw new LanNotFoundException { LanId = id };

        if (title is not null) lan.ChangeTitle(title, initiator);
        if (background is not null) lan.ChangeBackground(background, initiator);

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
