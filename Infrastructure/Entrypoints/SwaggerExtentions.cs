using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Seatpicker.Infrastructure.Entrypoints;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerGen(this IServiceCollection services)
    {
        services.AddSwaggerGen(
            options =>
            {
                options.SchemaGeneratorOptions.SupportNonNullableReferenceTypes = true;
                options.OperationFilter<CustomOperationFilter>();
                options.CustomSchemaIds(CreateCustomSchemeId);


                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    BearerFormat = "JWT",
                    Name = "Bearer token authentication",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    Description = "Bearer token acquired through the Discord authentication endpoints",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme, Type = ReferenceType.SecurityScheme,
                    },
                };

                options.AddSecurityDefinition("Bearer", jwtSecurityScheme);
                options.AddSecurityRequirement(
                    new OpenApiSecurityRequirement { { jwtSecurityScheme, Array.Empty<string>() } });
            });

        return services;
    }

    public static IApplicationBuilder UseSwaggerGen(this IApplicationBuilder app)
    {
        return app
            .UseSwagger()
            .UseSwaggerUI();
    }

    private static string CreateCustomSchemeId(Type type)
    {
        if (type.IsNested)
        {
            var declaringType = type.DeclaringType!;
            if (declaringType.Name.EndsWith("Controller")) return type.Name;

            var name = declaringType.Name + type.Name;
            var httpNamespace = typeof(EntrypointsExtensions).Namespace! + "." + nameof(Http);
            if (declaringType.Namespace!.StartsWith(httpNamespace))
            {
                name = declaringType.Namespace.Substring(httpNamespace.Length + 1) + name;
            }

            return name;
        }

        return type.Name;
    }

    public class CustomOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var path = context.ApiDescription.RelativePath!.Split('/');

            var stack = new Stack<string>();
            foreach (var section in path)
            {
                if (!Regex.IsMatch(section, @"^[a-zA-Z]+$")) break;
                stack.Push(section);
            }
            if (stack.Count > 1) stack.Pop();

            var name = string.Join('/', stack);

            operation.OperationId = context.ApiDescription.ActionDescriptor.DisplayName;
            operation.Tags.Clear();
            operation.Tags.Add(new OpenApiTag
            {
                Name = name,
            });
        }
    }
}