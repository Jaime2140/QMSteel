using QMSteel.Api.DTOs;
using QMSteel.Application.Abstractions;

namespace QMSteel.Api.Endpoints.Files
{
    public static class DownloadFileEndpoint
    {
        private const string ENDPOINT_NAME = "Descargar Archivo";

        public static RouteGroupBuilder MapDownloadFile(this RouteGroupBuilder app)
        {
            app.MapGet("archivos/{id:guid}", async (Guid id, IFileStorage fileStorage, CancellationToken ct) =>
            {
                var stored = await fileStorage.OpenAsync(id, ct);
                if (stored is not { } found)
                    return Results.Json(new ErrorResponse(StatusCodes.Status404NotFound, "Registro no encontrado", "FILE_NOT_FOUND",
                        "El archivo no existe."), statusCode: StatusCodes.Status404NotFound);

                return Results.File(found.Content, found.File.ContentType, found.File.FileName);
            })
            .Produces(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Descarga un archivo de la planta en sesión")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
