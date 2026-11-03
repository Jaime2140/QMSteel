using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Procedures;
using QMSteel.Domain.Welding;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class GetWpsMatrixEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Matriz WPS PQR";

        public static RouteGroupBuilder MapGetWpsMatrix(this RouteGroupBuilder app)
        {
            app.MapGet("wps/matriz", async ([FromQuery(Name = "proceso")] WeldingProcess? process, ProcedureService procedureService, CancellationToken ct) =>
            {
                var response = await procedureService.GetMatrixAsync(process, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<WpsMatrixRowDto>>(StatusCodes.Status200OK)
            .WithDescription("Matriz de WPS con sus PQR de respaldo y las discrepancias de variables esenciales")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
