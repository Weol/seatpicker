using Marten;
using Seatpicker.Application.Features;
using Shared;
using System.Linq;

namespace Seatpicker.Infrastructure.Adapters.Database.GuildHostMapping;

public class GuildHostMappingRepository
{
    private readonly DocumentRepository documentRepository;

    public GuildHostMappingRepository(DocumentRepository documentRepository)
    {
        this.documentRepository = documentRepository;
    }

    public async Task Save(ICollection<(string GuildId, string[] Hostnames)> mappings)
    {
        using var transaction = documentRepository.CreateGlobalTransaction();
        
        var duplicateHosts = mappings
            .SelectMany(mapping => mapping.Hostnames)
            .GroupBy(host => host)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateHosts.Length > 0)
        {
            throw new DuplicateHostsException
            {
                DuplicateHosts = duplicateHosts
            }; 
        }
        
        // Yes, we delete ALL of the mappings
        transaction.DeleteWhere<GuildHostMapping>(mapping => true);

        foreach (var (guildId, hostnames) in mappings)
        {
            foreach (var hostname in hostnames)
            {
                transaction.Store(new GuildHostMapping(hostname, guildId));
            }
        }

        await transaction.Commit();
    }

    public async IAsyncEnumerable<(string GuildId, IEnumerable<string> Hostnames)> GetAll()
    {
        using var reader = documentRepository.CreateGlobalReader();

        var mappings = reader.Query<GuildHostMapping>()
            .ToAsyncEnumerable()
            .GroupBy(mapping => mapping.GuildId, mapping => mapping.Hostname);

        await foreach (var grouping in mappings)
        {
            yield return (grouping.Key, await grouping.ToArrayAsync());
        }
    }

    public async Task<string?> GetGuildIdByHost(string host)
    {
        using var reader = documentRepository.CreateReader();

        var mapping = await reader.Get<GuildHostMapping>(host);

        return mapping?.GuildId;
    }

    public class DuplicateHostsException : Exception
    {
        public IEnumerable<string> DuplicateHosts { get; init; }
    }
}

public record GuildHostMapping(string Hostname, string GuildId) : IGlobalDocument
{
    public string Id => Hostname;
}