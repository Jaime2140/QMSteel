using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using QMSteel.Api.Endpoints.Alerts;
using QMSteel.Api.Endpoints.Audits;
using QMSteel.Api.Endpoints.Compliance;
using QMSteel.Api.Endpoints.Dashboard;
using QMSteel.Api.Endpoints.Documents;
using QMSteel.Api.Endpoints.Equipments;
using QMSteel.Api.Endpoints.Files;
using QMSteel.Api.Endpoints.Inspections;
using QMSteel.Api.Endpoints.Inspectors;
using QMSteel.Api.Endpoints.ManagementReviews;
using QMSteel.Api.Endpoints.NonConformities;
using QMSteel.Api.Endpoints.Procedures;
using QMSteel.Api.Endpoints.QualityObjectives;
using QMSteel.Api.Endpoints.Receiving;
using QMSteel.Api.Endpoints.Suppliers;
using QMSteel.Api.Endpoints.Tenants;
using QMSteel.Api.Endpoints.Training;
using QMSteel.Api.Endpoints.Users;
using QMSteel.Api.Endpoints.Welders;
using QMSteel.Application.Bootstrap;
using QMSteel.Infrastructure;

namespace QMSteel.Api.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication MapEndpoints(this WebApplication app)
        {
            var apiEndpoints = app.MapGroup("api");

            var protectedEndpoints = apiEndpoints.MapGroup("").RequireAuthorization();
            protectedEndpoints.MapUserEndpoints();
            protectedEndpoints.MapTenantEndpoints();
            protectedEndpoints.MapFileEndpoints();
            protectedEndpoints.MapDocumentEndpoints();
            protectedEndpoints.MapEquipmentEndpoints();
            protectedEndpoints.MapWelderEndpoints();
            protectedEndpoints.MapProcedureEndpoints();
            protectedEndpoints.MapInspectorEndpoints();
            protectedEndpoints.MapTrainingEndpoints();
            protectedEndpoints.MapSupplierEndpoints();
            protectedEndpoints.MapReceivingEndpoints();
            protectedEndpoints.MapNonConformityEndpoints();
            protectedEndpoints.MapAuditEndpoints();
            protectedEndpoints.MapManagementReviewEndpoints();
            protectedEndpoints.MapInspectionEndpoints();
            protectedEndpoints.MapComplianceEndpoints();
            protectedEndpoints.MapQualityObjectiveEndpoints();
            protectedEndpoints.MapDashboardEndpoints();
            protectedEndpoints.MapAlertEndpoints();

            apiEndpoints.AddOpenApi();
            return app;
        }

        // Crea el esquema (SQLite) o aplica migraciones (PostgreSQL) y da de alta la planta inicial si está configurada.
        public static async Task<WebApplication> InicializarBaseDeDatosAsync(this WebApplication app)
        {
            await app.Services.InitializeDatabaseAsync();

            await using var scope = app.Services.CreateAsyncScope();
            var options = scope.ServiceProvider.GetRequiredService<IOptions<BootstrapOptions>>().Value;
            await scope.ServiceProvider.GetRequiredService<TenantBootstrapper>().RunAsync(options);
            return app;
        }

        private static RouteGroupBuilder AddOpenApi(this RouteGroupBuilder group)
        {
            return group.AddOpenApiOperationTransformer((options, context, ct) =>
            {
                options.Responses ??= new OpenApiResponses();
                options.Responses["400"] = new OpenApiResponse { Description = "Solicitud incorrecta" };
                options.Responses["401"] = new OpenApiResponse { Description = "No autenticado" };
                options.Responses["403"] = new OpenApiResponse { Description = "Acceso denegado" };
                options.Responses["500"] = new OpenApiResponse { Description = "Error interno del servidor" };
                return Task.CompletedTask;
            });
        }
    }
}
