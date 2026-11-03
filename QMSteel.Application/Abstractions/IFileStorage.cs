namespace QMSteel.Application.Abstractions
{
    public sealed record StoredFile(Guid Id, string FileName, string ContentType, long Length);

    // Almacén de evidencias (PDF de certificados, WPS, MTR, fotos). Las entidades solo guardan el Id del archivo.
    public interface IFileStorage
    {
        Task<StoredFile> SaveAsync(Stream content, string fileName, string contentType, CancellationToken ct = default);

        // Null si el archivo no existe o pertenece a otra planta.
        Task<(StoredFile File, Stream Content)?> OpenAsync(Guid id, CancellationToken ct = default);
    }
}
