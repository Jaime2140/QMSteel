using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Application.Abstractions;

namespace QMSteel.Api.Endpoints.Files
{
    public static class UploadFileEndpoint
    {
        private const string ENDPOINT_NAME = "Subir Archivo";
        private const long MAX_BYTES = 25 * 1024 * 1024;
        private static readonly string[] AllowedExtensions = [".pdf", ".png", ".jpg", ".jpeg"];

        public static RouteGroupBuilder MapUploadFile(this RouteGroupBuilder app)
        {
            app.MapPost("archivos", async ([FromForm(Name = "archivo")] IFormFile file, IFileStorage fileStorage, CancellationToken ct) =>
            {
                if (file.Length == 0 || file.Length > MAX_BYTES)
                    return Invalid("FILE_SIZE_INVALID", "El archivo está vacío o excede 25 MB.");
                if (!AllowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
                    return Invalid("FILE_TYPE_INVALID", "Solo se aceptan archivos PDF, PNG o JPG.");

                await using var content = file.OpenReadStream();
                var stored = await fileStorage.SaveAsync(content, file.FileName, file.ContentType, ct);
                return Results.Created($"/api/archivos/{stored.Id}", stored);
            })
            .DisableAntiforgery()
            .Produces<StoredFile>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .WithDescription("Sube un PDF o una imagen (máx. 25 MB) y regresa el Id para ligarlo a certificados, WPS, MTR o evidencias")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }

        private static IResult Invalid(string code, string message) =>
            Results.Json(new ErrorResponse(StatusCodes.Status400BadRequest, "Solicitud inválida", code, message),
                statusCode: StatusCodes.Status400BadRequest);
    }
}
