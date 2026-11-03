using System.Text.Json;
using QMSteel.Application.Abstractions;

namespace QMSteel.Infrastructure.Files
{
    // Guarda los archivos en disco, una carpeta por planta: {raíz}/{tenant}/{id}.bin y sus datos en {id}.json.
    // Sirve para desarrollo y para un servidor propio; en la nube se reemplaza por un almacenamiento de objetos.
    internal sealed class LocalFileStorage : IFileStorage
    {
        private readonly string _rootPath;
        private readonly ICurrentUserContext _currentUser;

        public LocalFileStorage(string rootPath, ICurrentUserContext currentUser)
        {
            _rootPath = rootPath;
            _currentUser = currentUser;
        }

        public async Task<StoredFile> SaveAsync(Stream content, string fileName, string contentType, CancellationToken ct = default)
        {
            var folder = TenantFolder();
            Directory.CreateDirectory(folder);

            var id = Guid.NewGuid();
            await using (var target = File.Create(DataPath(folder, id)))
                await content.CopyToAsync(target, ct);

            var file = new StoredFile(id, Path.GetFileName(fileName), contentType, new FileInfo(DataPath(folder, id)).Length);
            await File.WriteAllTextAsync(MetadataPath(folder, id), JsonSerializer.Serialize(file), ct);
            return file;
        }

        public async Task<(StoredFile File, Stream Content)?> OpenAsync(Guid id, CancellationToken ct = default)
        {
            var folder = TenantFolder();
            if (!File.Exists(MetadataPath(folder, id)) || !File.Exists(DataPath(folder, id)))
                return null;

            var file = JsonSerializer.Deserialize<StoredFile>(await File.ReadAllTextAsync(MetadataPath(folder, id), ct))!;
            return (file, File.OpenRead(DataPath(folder, id)));
        }

        private string TenantFolder()
        {
            var tenantId = _currentUser.TenantId ?? throw new InvalidOperationException("La petición no tiene tenant.");
            return Path.Combine(_rootPath, tenantId.ToString("N"));
        }

        private static string DataPath(string folder, Guid id) => Path.Combine(folder, $"{id:N}.bin");

        private static string MetadataPath(string folder, Guid id) => Path.Combine(folder, $"{id:N}.json");
    }
}
