using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features;
using Seatpicker.Application.Features.Lan;
using Seatpicker.Infrastructure.Adapters.Database;

namespace Seatpicker.Infrastructure.Entrypoints.GraphQL.GuildQueries;

public class Mutations(UnitOfWorkFactory unitOfWorkFactory, DocumentRepository documentRepository)
{
    public async Task<ProjectedLan?> CreateLan([FromRoute] string guildId, CreateLan model)
    {
        var id = await unitOfWorkFactory.Create<string>(guildId,
            async unitOfWork => await unitOfWork.Lan.Create(model.Title, model.Background));

        var reader = documentRepository.CreateReader(guildId);

        var lan = await reader.Query<ProjectedLan>(id);
        return lan;
    }


}

public record CreateLan(string Id, string Title, byte[] Background);