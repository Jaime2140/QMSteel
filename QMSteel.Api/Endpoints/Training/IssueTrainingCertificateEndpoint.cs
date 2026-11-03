using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Training;

namespace QMSteel.Api.Endpoints.Training
{
    public static class IssueTrainingCertificateEndpoint
    {
        private const string ENDPOINT_NAME = "Expedir Constancia";

        public static RouteGroupBuilder MapIssueTrainingCertificate(this RouteGroupBuilder app)
        {
            app.MapPost("capacitacion/sesiones/{id:guid}/participantes/{employeeNumber}/constancia", async (Guid id, string employeeNumber, TrainingService trainingService, CancellationToken ct) =>
            {
                var result = await trainingService.IssueCertificateAsync(id, employeeNumber, ct);
                return result.ToHttpResult();
            })
            .Produces<TrainingSessionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Expide la constancia interna (DC-3 / diploma) con folio consecutivo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
