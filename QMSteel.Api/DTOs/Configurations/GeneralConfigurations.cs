using QMSteel.Infrastructure.Persistence;

namespace QMSteel.Api.DTOs.Configurations
{
    // La cadena de conexión con credenciales va en user-secrets o en variables de entorno, nunca en appsettings.
    public sealed class GeneralConfigurations
    {
        public const string SectionName = "GeneralConfigurations";

        public DatabaseProvider DataBaseProvider { get; set; } = DatabaseProvider.Sqlite;
        public string ConnectionString { get; set; } = string.Empty;
        public string FileStorageRoot { get; set; } = "storage";
    }
}
