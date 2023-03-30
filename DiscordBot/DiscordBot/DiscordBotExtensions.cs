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

    private static IServiceCollection AddDiscordEventHandlers(this IServiceCollection services)
    {
        var events = typeof(Program).Assembly.GetTypes().Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(IDiscordEvent)));
        foreach (var evt in events)
        {
            var consumers = typeof(Program).Assembly.GetTypes().Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(IDiscordEventHandler<>)));
            foreach (var consumer in consumers)
            {
                var interfaces = consumer.GetInterfaces();
            }
        }

        return services;
    }

    [SuppressMessage("ReSharper", "ArrangeObjectCreationWhenTypeNotEvident")]
    private static DiscordSocketClient CreateDiscordSocketClient(IServiceProvider provider)
    {
        var client = new DiscordSocketClient();

        client.MessageReceived += ForAll<SocketMessage, MessageReceivedEvent>(provider, message => new (message));

        return client;
    }

    private static Func<T, Task> ForAll<T, G>(IServiceProvider provider, Func<T, G> creator)
        where G : IDiscordEvent
    {
        var handlers  = provider.GetHandlers<G>();

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