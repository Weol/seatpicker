using Marten;
using Seatpicker.Application.Features;
using Shared;
using System.Linq;

namespace Seatpicker.Infrastructure.Adapters.Database.GuildHostMapping;

public class GuildHostMappingRepository
{
    private readonly IDocumentRepository documentRepository;

    public GuildHostMappingRepository(IDocumentRepository documentRepository)
    {
        this.documentRepository = documentRepository;
    }

    public async Task Save(string guildId, IEnumerable<string> hostnames)
    {
        using var transaction = documentRepository.CreateTransaction();

        foreach (var hostname in hostnames)
        {
            transaction.Store(new GuildHostMapping(hostname, guildId));
        }

        await transaction.Commit();
    }

    public async IAsyncEnumerable<(string GuildId, IEnumerable<string> Hostnames)> GetAll()
    {
        using var reader = documentRepository.CreateReader();

        var mappings = reader.Query<GuildHostMapping>()
            .GroupBy(mapping => mapping.GuildId, mapping => mapping.Hostname)
            .ToAsyncEnumerable();
        
        await foreach (var grouping in mappings)
        {
            yield return (grouping.Key, grouping);
        }
    }
    
    public class HostnamesAlreadyMappedException : Exception
    {
        public IEnumerable<string> Hostnames { get; init; }
    }
}

public record GuildHostMapping(string Hostname, string GuildId) : IDocument
{
    public string Id => Hostname;
}