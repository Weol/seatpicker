using Discord;
using Discord.WebSocket;
using DiscordBot.DiscordBot;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.local.json", true);

builder
    .AddDiscordBot();

var app = builder.Build();

app.Run();