using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Discord.WebSocket;
using DiscordBot.Handlers;

namespace DiscordBot.DiscordBot;

public static class DiscordBotExtensions
{
    public static WebApplicationBuilder AddDiscordBot(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<DiscordBotOptions>()
            .Configure<IConfiguration>(
            (options, configuration) =>
            {
                options.Token = configuration["DiscordBotToken"];
            });

        builder.Services
            .AddDiscordEventHandlers()
            .AddSingleton(CreateDiscordSocketClient)
            .AddHostedService<DiscordBot>();

        return builder;
    }

    [SuppressMessage("ReSharper", "ArrangeObjectCreationWhenTypeNotEvident")]
    private static DiscordSocketClient CreateDiscordSocketClient(IServiceProvider provider)
    {
        var client = new DiscordSocketClient();

        client.MessageReceived += ForAll<SocketMessage, MessageReceivedEvent>(provider, message => new (message));

        return client;
    }

    private static IServiceCollection AddDiscordEventHandlers(this IServiceCollection services)
    {
        var events = typeof(Program).Assembly.GetTypes().Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(IDiscordEvent)));
        foreach (var evt in events)
        {
            var mi = typeof(DiscordBotExtensions).GetMethod(nameof(AddHandlers));
            mi!.MakeGenericMethod(evt).Invoke(null, new object?[] { services });
        }

        return services;
    }

    public static void AddHandlers<T>(IServiceCollection services)
        where T : IDiscordEvent
    {
        var consumers = typeof(Program).Assembly.GetTypes().Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(IDiscordEventHandler<T>)));
        foreach (var consumer in consumers)
        {
            services.Add(new ServiceDescriptor(typeof(IDiscordEventHandler<T>), consumer, ServiceLifetime.Transient));
        }
    }

    private static Func<T, Task> ForAll<T, G>(IServiceProvider provider, Func<T, G> creator)
        where G : IDiscordEvent
    {
        var handlers  = provider.GetHandlers<G>();
        var logger = provider.GetRequiredService<ILogger<DiscordBot>>();

        return t =>
        {
            var message = creator(t);
            return Task.WhenAll(handlers.Select(handler => handler.Handle(message)));
        };
    }

    private static IEnumerable<IDiscordEventHandler<T>> GetHandlers<T>(this IServiceProvider provider)
        where T : IDiscordEvent
    {
        return provider.GetRequiredService<IEnumerable<IDiscordEventHandler<T>>>();
    }
}