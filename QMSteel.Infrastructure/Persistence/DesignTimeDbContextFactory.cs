using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using QMSteel.Application.Abstractions;

namespace QMSteel.Infrastructure.Persistence
{
    // Lo usa `dotnet ef` para crear las migraciones de PostgreSQL sin levantar la API (SQLite no las usa).
    // Generar una migración no abre conexión; aplicarla sí: define QMSTEEL_CONNECTION con tu cadena local.
    internal class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<QMSteelDbContext>
    {
        public QMSteelDbContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("QMSTEEL_CONNECTION")
                ?? "Host=localhost;Database=qmsteel;Username=postgres";

            var options = new DbContextOptionsBuilder<QMSteelDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new QMSteelDbContext(options, new NoUserContext(), TimeProvider.System);
        }

        private sealed class NoUserContext : ICurrentUserContext
        {
            public Guid? UserId => null;
            public Guid? TenantId => null;
        }
    }
}
