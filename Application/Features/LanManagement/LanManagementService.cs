using Seatpicker.Domain;

namespace Seatpicker.Application.Features.LanManagement;

public interface ILanManagementService
{
    public Task Create(CreateLan createLan);

    public Task Update(UpdateLan updateLan);

    public Task<Domain.Lan> Get(Guid lanId);
}

internal class LanManagementManagementService : ILanManagementService
{
    private readonly IAggregateRepository repository;

    public LanManagementManagementService(IAggregateRepository repository)
    {
        this.repository = repository;
    }

    public async Task Create(CreateLan createLan)
    {
        await using var transaction = repository.CreateTransaction();

        var exists = await transaction.Exists<Domain.Lan>(createLan.Id);
        if (exists) throw new LanAlreadyExistsException { LanId = createLan.Id };

        var lan = new Domain.Lan(createLan.Id, createLan.Title, createLan.Background);

        transaction.Create(lan);

        transaction.Commit();
    }

    public async Task Update(UpdateLan updateLan)
    {
        await using var transaction = repository.CreateTransaction();

        var lan = await transaction.Aggregate<Domain.Lan>(updateLan.Id);
        if (lan is null) throw new LanNotFoundException { LanId = updateLan.Id };

        if (updateLan.Title is not null) lan.ChangeTitle(updateLan.Title);
        if (updateLan.Background is not null) lan.ChangeBackground(updateLan.Background);

        transaction.Update(lan);
        transaction.Commit();
    }

    public async Task<Domain.Lan> Get(Guid lanId)
    {
        await using var reader = repository.CreateReader();

        var lan = await reader.Aggregate<Domain.Lan>(lanId);
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
