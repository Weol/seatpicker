using System.Collections.ObjectModel;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Seatpicker.Application.Features.Reservation.Ports;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Adapters;

internal class SeatRepository : ISeatRepository
{
    private readonly TableClient tableClient;

    public SeatRepository(IAzureClientFactory<TableServiceClient> tableClientFactory, IOptions<Options> options)
    {
        var tableServiceClient = tableClientFactory.CreateClient(options.Value.Endpoint);

        tableServiceClient.CreateTableIfNotExists(options.Value.TableName);

        tableClient = tableServiceClient
            .GetTableClient(options.Value.TableName);
    }

    public async Task Store(Seat seat)
    {
        if (seat.User == null) throw new ApplicationException("Cannot store seat without user");
        await tableClient.UpsertEntityAsync(new SeatEntity
        {
            RowKey= seat.Id.ToString(),
            UserId = seat.User.Id,
            UserNick = seat.User.Nick,
            UserAvatar = seat.User.Avatar,
            ReservedAt = DateTimeOffset.UtcNow,
        });
    }

    public async Task<ICollection<Seat>> GetAll()
    {
        var enumerator = tableClient.QueryAsync<SeatEntity>()
            .GetAsyncEnumerator();

        if (enumerator.Current is null) return Array.Empty<Seat>();

        var seats = new Collection<Seat>();
        do
        {
            var entity = enumerator.Current;
            seats.Add(entity.ToSeat());
        } while (await enumerator.MoveNextAsync());

        return seats;
    }

    public async Task<Seat?> Get(Guid seatId)
    {
        try
        {
            var response = await tableClient.GetEntityIfExistsAsync<SeatEntity>(
                "Default",
                seatId.ToString());

            if (!response.HasValue) return null;
            var entity = response.Value;

            return entity.ToSeat();
        }
        catch (RequestFailedException e) when (e.Status == 404)
        {
            return null;
        }
    }

    private class SeatEntity : ITableEntity
    {
        public string RowKey { get; set; } = null!;
        public string PartitionKey { get; set; } = "Default";
        public string UserId { get; set; } = null!;
        public string UserNick { get; set; } = null!;
        public string UserAvatar { get; set; } = null!;
        public DateTimeOffset ReservedAt { get; set; }
        public ETag ETag { get; set; }

        public DateTimeOffset? Timestamp
        {
            get => DateTimeOffset.UtcNow;
            set {}
        }

        public Seat ToSeat()
        {
            var user = new User(UserId, UserNick, UserAvatar);

            return new Seat
            {
                Id = Guid.Parse(RowKey),
                User = user,
                ReservedAt = ReservedAt.UtcDateTime,
            };
        }
    }

    public class Options
    {
        public string Endpoint { get; set; } = null!;
        public string TableName { get; set; } = null!;
    }
}

internal static class SeatRepositoryExtensions
{
    internal static IServiceCollection AddSeatRepository(this IServiceCollection services, Action<SeatRepository.Options> configureAction)
    {
        services.Configure(configureAction);

        services.AddAzureClients(builder =>
        {
            var options = new SeatRepository.Options();
            configureAction(options);

            builder.AddTableServiceClient(options.Endpoint)
                .WithName(options.Endpoint);
        });

        services.AddSingleton<ISeatRepository, SeatRepository>();

        return services;
    }
}
