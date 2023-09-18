using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Seatpicker.Application;
using Seatpicker.Infrastructure;
using Seatpicker.Infrastructure.Adapters;
using Seatpicker.Infrastructure.Authentication;
using Seatpicker.Infrastructure.Entrypoints;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment()) builder.Configuration.AddJsonFile("appsettings.local.json");

builder.Configuration
    .AddSeatpickerKeyvault()
    .AddEnvironmentVariables("App_");

builder.Services.AddApplicationInsightsTelemetry()
    .AddAdapters()
    .AddSeatpickerAuthentication()
    .ConfigureJsonSerialization()
    .AddEntrypoints(builder.Configuration)
    .AddApplication()
    .AddSwaggerGen(
        options =>
        {
            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Name = "Bearer token authentication",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Description = "Bearer token",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme, Type = ReferenceType.SecurityScheme,
                },
            };

            options.AddSecurityDefinition("Bearer", jwtSecurityScheme);
            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement { { jwtSecurityScheme, Array.Empty<string>() } });
        });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseEntrypoints();
app.UseSeatpickerAuthentication();

app.Run();

namespace Seatpicker.Infrastructure
{
    public partial class Program
    {
    }
}