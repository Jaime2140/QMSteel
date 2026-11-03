using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using QMSteel.Api.DTOs.Configurations;
using QMSteel.Api.Security;
using QMSteel.Application;
using QMSteel.Application.Abstractions;
using QMSteel.Application.Bootstrap;
using QMSteel.Infrastructure;

namespace QMSteel.Api.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection InyectarServicios(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplication();

            services.AddScoped<HttpCurrentUserContext>();
            services.AddScoped<ICurrentUserContext>(sp => sp.GetRequiredService<HttpCurrentUserContext>());

            services.Configure<BootstrapOptions>(configuration.GetSection(BootstrapOptions.SectionName));

            // Enums como texto en el JSON ("SMAW", "Approved"...), igual que en la base de datos.
            services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            return services;
        }

        public static IServiceCollection InyectarContextos(this IServiceCollection services, GeneralConfigurations config)
        {
            services.AddInfrastructure(config.DataBaseProvider, config.ConnectionString, config.FileStorageRoot);
            return services;
        }

        // Los tokens los emite el proveedor de identidad; la configuración se lee de "Authentication:Schemes:Bearer"
        // (Authority, ValidAudiences, ValidIssuer). En desarrollo se generan con `dotnet user-jwts`.
        public static IServiceCollection ConfigureAuthentication(this IServiceCollection services)
        {
            services.AddAuthorization();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
            return services;
        }

        public static IServiceCollection ConfigurarSwagger(this IServiceCollection services)
        {
            services.AddOpenApi(options => options.AddDocumentTransformer((document, context, ct) =>
            {
                document.Info.Title = "QMSteel API";

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Token del proveedor de identidad"
                };
                document.Security =
                [
                    new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = [] }
                ];
                return Task.CompletedTask;
            }));

            return services;
        }
    }
}
