using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Seatpicker.Infrastructure.Adapters.Database.GuildHostMapping;
using System.Net;
using System.Linq;

namespace Seatpicker.Infrastructure.Entrypoints.Http.Guild;

public static class PutHostMapping
{
    public static async Task<IResult> Put(
        [FromServices] GuildHostMappingRepository guildHostMappingRepository,
        [FromBody] IEnumerable<Request> request)
    {
        var mappings = request.Select(x => (x.GuildId, x.Hostnames))
            .ToArray();

        try
        {
            await guildHostMappingRepository.Save(mappings);
        }
        catch (GuildHostMappingRepository.DuplicateHostsException e)
        {
            var duplicates = string.Join(", ", e.DuplicateHosts);
            return Results.UnprocessableEntity($"A host can only be mapped once, duplicate hosts: {duplicates}");
        }

        return Results.Ok();
    }

    public record Request(string GuildId, string[] Hostnames);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GuildId).NotEmpty();

            RuleForEach(x => x.Hostnames)
                .NotEmpty()
                .Must(IsValidDomain)
                .WithMessage("All hostnames must be valid DNS hostnames");

            RuleFor(x => x.Hostnames)
                .Must(x => x.Distinct().Count() == x.Length)
                .WithMessage("No duplicate hostnames are allowed");
        }

        private bool IsValidDomain(string hostname)
        {
            return Uri.CheckHostName(hostname) != UriHostNameType.Unknown;
        }
    }
}