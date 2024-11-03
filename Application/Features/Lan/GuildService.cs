namespace Seatpicker.Application.Features.Lan;

public class GuildService
{
    private readonly IGuildlessDocumentTransaction documentTransaction;
    private readonly IDocumentReader documentReader;

    public GuildService(IDiscordGuildProvider discordGuildProvider,
        IGuildlessDocumentTransaction documentTransaction,
        IDocumentReader documentReader)
    {
        this.documentTransaction = documentTransaction;
        this.documentReader = documentReader;
    }

    public Task<Guild> Update(Guild guild)
    {
        var duplicateHosts = documentReader.Query<Guild>()
            .Where(document => document.Hostnames.Any(hostname => guild.Hostnames.Contains(hostname)))
            .AsEnumerable()
            .SelectMany(document => document.Hostnames.Intersect(guild.Hostnames))
            .ToArray();

        if (duplicateHosts.Length > 0)
        {
            throw new DuplicateHostsException(duplicateHosts);
        }

        documentTransaction.Store(guild);

        return Task.FromResult(guild);
    }

    public class DuplicateHostsException(IEnumerable<string> duplicateHosts) : Exception
    {
        public IEnumerable<string> DuplicateHosts { get; init; } = duplicateHosts;
    }
}