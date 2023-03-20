using System.Collections.ObjectModel;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Seatpicker.Application.Features;
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
        await tableClient.UpsertEntityAsync(new SeatEntity
        {
            RowKey= seat.Id.ToString(),
            UserId = seat.User?.Id ?? "",
            UserNick = seat.User?.Nick ?? "",
            UserAvatar = seat.User?.Avatar ?? "",
            Title = seat.Title,
            X = seat.X,
            Y = seat.Y,
            Width = seat.Width,
            Height = seat.Height,
            ReservedAt = DateTimeOffset.UtcNow,
        });
    }

    public async Task<ICollection<Seat>> GetAll()
    {
        var enumerator = tableClient.QueryAsync<SeatEntity>()
            .GetAsyncEnumerator();

        await enumerator.MoveNextAsync();
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

    public async Task<Seat?> GetByUser(string userId)
    {
        try
        {
            var enumerator = tableClient.QueryAsync<SeatEntity>(entity => entity.UserId == userId)
                .GetAsyncEnumerator();

            await enumerator.MoveNextAsync();
            return enumerator.Current?.ToSeat();
        }
        catch (RequestFailedException e) when (e.Status == 404)
        {
            return null;
        }
    }

    private record SeatEntity : ITableEntity
    {
        public string RowKey { get; set; } = null!;
        public string PartitionKey { get; set; } = "Default";
        public string? UserId { get; set; }
        public string? UserNick { get; set; }
        public string? UserAvatar { get; set; }
        public string Title { get; set; } = null!;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public DateTimeOffset ReservedAt { get; set; }
        public ETag ETag { get; set; }

        public DateTimeOffset? Timestamp
        {
            get => DateTimeOffset.UtcNow;
            set {}
        }

        public Seat ToSeat()
        {
            var user = GetUser();

            return new Seat
            {
                Id = Guid.Parse(RowKey),
                User = user,
                Title = Title,
                X = X,
                Y = Y,
                Width = Width,
                Height = Height,
                ReservedAt = ReservedAt.UtcDateTime,
            };
        }

        private User? GetUser()
        {
            if (UserId is null || UserNick is null || UserAvatar is null) return null;
            if (UserId.IsNullOrEmpty() || UserNick.IsNullOrEmpty() || UserAvatar.IsNullOrEmpty()) return null;

            return new User {
                Id = UserId,
                Nick = UserNick,
                Avatar = UserAvatar,
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
