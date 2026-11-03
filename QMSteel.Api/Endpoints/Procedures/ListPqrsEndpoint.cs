using QMSteel.Application.Procedures;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class ListPqrsEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar PQRs";

        public static RouteGroupBuilder MapListPqrs(this RouteGroupBuilder app)
        {
            app.MapGet("pqr", async (ProcedureService procedureService, CancellationToken ct) =>
            {
                var response = await procedureService.ListPqrsAsync(ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<PqrDto>>(StatusCodes.Status200OK)
            .WithDescription("Lista los registros de calificación de procedimiento")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
