using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Adapters.Common;
using Seatpicker.Domain;
using Seatpicker.Domain.Application.UserToken.Ports;
using Seatpicker.Domain.Domain;
using Seatpicker.Domain.Domain.Registration.Ports;

namespace Seatpicker.Adapters.Adapters;

internal class UserStore : IStoreUser, ILookupUser
{
    private readonly TableClient tableClient;

    public UserStore(TableClient tableClient)
    {
        this.tableClient = tableClient;
    }

    public async Task Store(User user)
    {
        var jsonRoles = JsonSerializer.Serialize(user.Roles);

        await tableClient.UpsertEntityAsync(new UserEntity
        {
            RowKey= user.Id,
            Nick = user.Nick,
            Name = user.Name,
            Avatar = user.Avatar,
            Roles = jsonRoles,
            CreatedAt = user.CreatedAt
        });
    }
    
    public async Task<User?> Lookup(string id)
    {
        try
        {
            var userEntity = await tableClient.GetEntityAsync<UserEntity>("Default", id);
            var entity = userEntity.Value;

            var roles = JsonSerializer.Deserialize<IEnumerable<Role>>(entity.Roles) ?? throw new NullReferenceException();

            return new User(
                entity.RowKey,
                entity.Nick,
                entity.Avatar,
                entity.Name,
                roles,
                entity.CreatedAt);
        }
        catch (RequestFailedException e) when (e.Status == 404)
        {
            return null;
        }
    }

    private class UserEntity : ITableEntity 
    {
        public string RowKey { get; set; }
        public string PartitionKey { get; set; } = "Default";
        public string Nick { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Avatar { get; set; } = null!;
        public string Roles { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? Timestamp { get => DateTimeOffset.Now; set => throw new NotSupportedException(); }
        
        public ETag ETag { get; set; }
    }
}

internal static class UserStoreExtensions
{
    internal static IServiceCollection AddUserStore(this IServiceCollection services, TableStorageOptions options)
    {
        services.AddAzureClients(builder =>
        {
            builder.AddTableServiceClient(options.Endpoint)
                .WithName(options.Endpoint);
        });

        return services;
    }
}
