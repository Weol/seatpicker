using Microsoft.AspNetCore.Mvc;
using Seatpicker.Application.Features;
using Seatpicker.Application.Features.Lan;

namespace Seatpicker.Infrastructure.Entrypoints.GraphQL;

public class Mutation(UnitOfWorkFactory unitOfWorkFactory, IDocumentRepository documentRepository)
{
    public async Task<ProjectedLan?> UpsertLan(LanDto dto)
    {
        var id = await unitOfWorkFactory.Create<string>("123",
            async unitOfWork => await unitOfWork.Lan.Create(dto.Title, dto.Background));

        var reader = documentRepository.CreateReader("123");

        var lan = await reader.Query<ProjectedLan>(id);
        return lan;
    }
}

public record LanDto(string Title, byte[] Background);