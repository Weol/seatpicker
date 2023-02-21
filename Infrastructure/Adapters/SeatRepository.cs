using System.Collections.ObjectModel;
using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
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

    public async Task Store(Reservation reservation)
    {
        if (reservation.User == null) throw new ApplicationException("Cannot store seat without user");
        await tableClient.UpsertEntityAsync(new SeatEntity
        {
            RowKey= reservation.Id.ToString(),
            UserId = reservation.User.Id,
            UserNick = reservation.User.Nick,
            UserAvatar = reservation.User.Avatar,
            ReservedAt = DateTimeOffset.UtcNow,
        });
    }

    public async Task<ICollection<Reservation>> GetAll()
    {
        var seats = new Collection<Reservation>();

        var enumerator = tableClient.QueryAsync<SeatEntity>()
            .GetAsyncEnumerator();

        do
        {
            var entity = enumerator.Current;
            seats.Add(entity.ToSeat());
        } while (await enumerator.MoveNextAsync());

        return seats;
    }

    public async Task<Reservation?> Get(Guid seatId)
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

        public Reservation ToSeat()
        {
            var user = new User(UserId, UserNick, UserAvatar);

            return new Reservation(Guid.Parse(RowKey), user, ReservedAt.UtcDateTime);
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
