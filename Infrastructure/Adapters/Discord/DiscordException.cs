using System.Net;

namespace Seatpicker.Infrastructure.Adapters.Discord;

#pragma warning disable CS1998
internal class DiscordException(string message) : Exception(message)
{
    public required HttpStatusCode StatusCode { get; init; }

    public required string Body { get; init; }
}