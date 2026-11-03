using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Procedures;
using QMSteel.Domain.Entities.Procedures;
using QMSteel.Domain.Welding;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class ListWpsEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Lista de WPS";

        public static RouteGroupBuilder MapListWps(this RouteGroupBuilder app)
        {
            app.MapGet("wps", async ([FromQuery(Name = "proceso")] WeldingProcess? process, [FromQuery(Name = "estatus")] WpsStatus? status, [FromQuery(Name = "junta")] JointType? jointType, ProcedureService procedureService, CancellationToken ct) =>
            {
                var response = await procedureService.ListWpsAsync(process, status, jointType, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<WpsDto>>(StatusCodes.Status200OK)
            .WithDescription("Base de datos de WPS con filtro por proceso, estatus y tipo de junta")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
