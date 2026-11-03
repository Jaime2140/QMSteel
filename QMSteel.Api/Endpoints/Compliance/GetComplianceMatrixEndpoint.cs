using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Compliance;
using QMSteel.Domain.Standards;

namespace QMSteel.Api.Endpoints.Compliance
{
    public static class GetComplianceMatrixEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Matriz de Cumplimiento";

        public static RouteGroupBuilder MapGetComplianceMatrix(this RouteGroupBuilder app)
        {
            app.MapGet("cumplimiento/{standard}", async (QualityStandard standard, [FromQuery(Name = "clausula")] string? clausePrefix, ComplianceService complianceService, CancellationToken ct) =>
            {
                var response = await complianceService.GetMatrixAsync(standard, clausePrefix, ct);
                return Results.Ok(response);
            })
            .Produces<ComplianceMatrixDto>(StatusCodes.Status200OK)
            .WithDescription("Porcentaje de cumplimiento de la norma; con cláusula muestra el detalle de ese punto")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
