using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Audits;

namespace QMSteel.Api.Endpoints.Audits
{
    public static class ListAuditsEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Auditorias";

        public static RouteGroupBuilder MapListAudits(this RouteGroupBuilder app)
        {
            app.MapGet("auditorias", async ([FromQuery(Name = "anio")] int year, AuditService auditService, CancellationToken ct) =>
            {
                var response = await auditService.ListByYearAsync(year, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<AuditSummaryDto>>(StatusCodes.Status200OK)
            .WithDescription("Calendario anual de auditorías internas y externas")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
