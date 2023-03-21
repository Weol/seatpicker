using System.Text.Json;
using System.Text.Json.Serialization;
using MassTransit;

namespace Seatpicker.Application.Features.Reservation.EventHandlers;

public class LogOnSeatReservedEvent : IConsumer<SeatReservedEvent>
{
    public Task Consume(ConsumeContext<SeatReservedEvent> context)
    {
        Console.WriteLine(JsonSerializer.Serialize(context.Message));

        return Task.CompletedTask;
    }
}