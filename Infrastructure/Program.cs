using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Seatpicker.Application;
using Seatpicker.Application.Features.Login.Ports;
using Seatpicker.Infrastructure.Adapters;
using Seatpicker.Infrastructure.Middleware;
using Seatpicker.Infrastructure.ModelValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables("App_");

if (builder.Environment.IsEnvironment("local"))
{
    builder.Configuration.AddJsonFile("appsettings.local.json");
}

builder.Services.AddControllers(options =>
{
    options.Filters.Add<HttpResponseExceptionFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<JwtBearerOptions>()
    .PostConfigure<IAuthCertificateProvider>(async (options, provider) =>
    {
        var certificate = await provider.Get();
        var key = new X509SecurityKey(certificate);
        options.TokenValidationParameters.IssuerSigningKey = key;
    });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();


var jsonSerializerOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
};

builder.Services
    .AddSingleton(jsonSerializerOptions)
    .AddModelValidator()
    .AddAdapters(builder.Configuration)
    .AddApplication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

namespace Seatpicker.Infrastructure
{
    public partial class Program {}
}