using Seatpicker.Application;
using Seatpicker.Infrastructure;
using Seatpicker.Infrastructure.Adapters;
using Seatpicker.Infrastructure.Entrypoints;
using Seatpicker.Infrastructure.Entrypoints.Http.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddEnvironmentVariables("App_");

if (builder.Environment.IsDevelopment()) builder.Configuration.AddJsonFile("appsettings.local.json");

builder.Services
    .AddApplicationInsightsTelemetry()
    .AddAdapters(builder.Configuration)
    .AddAuth()
    .ConfigureJsonSerialization()
    .AddLoggedInUserAccessor()
    .AddEntrypoints(builder.Configuration)
    .AddApplication();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

namespace Seatpicker.Infrastructure
{
    public partial class Program {}
}