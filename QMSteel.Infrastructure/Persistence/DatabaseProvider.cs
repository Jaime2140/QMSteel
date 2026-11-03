using Microsoft.EntityFrameworkCore;

namespace QMSteel.Infrastructure.Persistence
{
    // SQLite para desarrollo mientras no haya servidor; PostgreSQL es el motor destino de producción.
    public enum DatabaseProvider
    {
        Sqlite,
        PostgreSql
    }

    internal static class DatabaseProviderExtensions
    {
        public static DbContextOptionsBuilder UseQMSteelDatabase(this DbContextOptionsBuilder options, DatabaseProvider provider, string connectionString) =>
            provider switch
            {
                DatabaseProvider.Sqlite => options.UseSqlite(connectionString),
                DatabaseProvider.PostgreSql => options.UseNpgsql(connectionString),
                _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Proveedor de base de datos no soportado.")
            };
    }
}
