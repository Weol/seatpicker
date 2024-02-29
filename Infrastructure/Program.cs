using Microsoft.AspNetCore.Http.HttpResults;
using Seatpicker.Application;
using Seatpicker.Infrastructure;
using Seatpicker.Infrastructure.Adapters;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment()) builder.Configuration.AddJsonFile("appsettings.local.json");

builder.Configuration.AddEnvironmentVariables("App_");
builder.Configuration.AddSeatpickerKeyvault();

builder.Services.AddApplicationInsightsTelemetry()
    .AddLogging()
    .AddAdapters()
    .AddSeatpickerAuthentication()
    .ConfigureJsonSerialization()
    .AddEntrypoints(builder.Configuration)
    .AddApplication()
    .AddSwaggerGen();

var app = builder.Build();

app.UseSwaggerGen();
app.UseRouting();
app.UseSeatpickerAuthentication();
app.UseEntrypoints();
app.UseAdapters();

app.Run();

namespace Seatpicker.Infrastructure
{
    public class Program
    {
    }
}