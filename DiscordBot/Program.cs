using Discord;
using Discord.WebSocket;
using DiscordBot.DiscordBot;

var builder = WebApplication.CreateBuilder(args);

builder
    .AddDiscordBot();

var app = builder.Build();

var client = new DiscordSocketClient();

client.LoginAsync(TokenType.Bot, app.Configuration["DiscordBotToken"]).GetAwaiter().GetResult();

app.Run();