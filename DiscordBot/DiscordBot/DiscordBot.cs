using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace DiscordBot.DiscordBot;

public class DiscordBot : BackgroundService
{
    private readonly DiscordSocketClient discordSocketClient;
    private readonly ILogger<DiscordBot> logger;
    private readonly DiscordBotOptions options;

    private CancellationTokenSource? cancellationTokenSource;

    public DiscordBot(DiscordSocketClient discordSocketClient, IOptions<DiscordBotOptions> options, ILogger<DiscordBot> logger)
    {
        this.discordSocketClient = discordSocketClient;
        this.logger = logger;
        this.options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Initializing Discord bot");

        stoppingToken.Register(InterruptInfiniteWait);

        discordSocketClient.Disconnected += OnDisconnect;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Starting Discord bot");

                cancellationTokenSource = new CancellationTokenSource();

                await discordSocketClient.LoginAsync(TokenType.Bot, options.Token);
                await discordSocketClient.StartAsync();

                await Task.WhenAny(Task.Delay(Timeout.Infinite, cancellationTokenSource.Token));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Encountered exception on Discord bot startup");
            }
        }
    }

    private void InterruptInfiniteWait()
    {
        cancellationTokenSource?.Cancel();
    }

    private Task OnDisconnect(Exception exception)
    {
        logger.LogError(exception, "Discord bot disconnected");
        InterruptInfiniteWait();

        return Task.CompletedTask;
    }
}
