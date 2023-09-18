using Seatpicker.Application.Features;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Adapters.Database.Queries;

#pragma warning disable CS1998 // Async method with no await
public class LanQueries
{
    private readonly IAggregateReader reader;

    public LanQueries(IAggregateReader reader)
    {
        this.reader = reader;
    }

    public async Task<Lan?> GetLan(Guid lanId)
    {
        return await reader.Aggregate<Lan>(lanId);
    }

    public async Task<IEnumerable<Lan>> GetAllLan()
    {
        return reader.Query<Lan>().ToArray();
    }
}