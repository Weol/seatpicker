using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Seatpicker.Adapters.Common;
using Seatpicker.Domain;
using Seatpicker.Domain.UserRegistration.Ports;

namespace Seatpicker.Adapters.Adapters;

internal class UserStore : IStoreUser
{
    private readonly TableClient tableClient;

    public UserStore(TableClient tableClient)
    {
        this.tableClient = tableClient;
    }

    public async Task Store(User user)
    {
        var jsonClaims = JsonSerializer.Serialize(user.Claims);

        await tableClient.UpsertEntityAsync(new UserEntity
        {
            Id = user.Id,
            Nick = user.Nick,
            Name = user.Name,
            Claims = jsonClaims,
            CreatedAt = user.CreatedAt
        });
    }

    private class UserEntity : AbstractTableEntity
    {
        public override string Id { get; set; } = null!;
        public string Nick { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Claims { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
    }
}

internal static class UserStoreExtensions
{
    internal static IServiceCollection AddUserStore(this IServiceCollection services, TableStorageOptions options)
    {
        services.AddAzureClients(builder =>
        {
            builder.AddTableServiceClient(options.Uri)
                .WithName(options.Uri.ToString());
        });

        return services;
    }
}
