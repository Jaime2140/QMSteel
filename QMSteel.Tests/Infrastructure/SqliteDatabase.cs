using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QMSteel.Application.Abstractions;
using QMSteel.Infrastructure.Persistence;

namespace QMSteel.Tests.Infrastructure
{
    // Base SQLite en memoria: vive mientras la conexión siga abierta, así cada prueba arranca con un esquema limpio.
    internal sealed class SqliteDatabase : IDisposable
    {
        private readonly SqliteConnection _connection = new("DataSource=:memory:");

        public SqliteDatabase()
        {
            _connection.Open();
            using var context = CreateContext(new FakeCurrentUser());
            context.Database.EnsureCreated();
        }

        // Cada contexto nuevo simula otra petición: no comparte el rastreo de cambios con el anterior.
        public QMSteelDbContext CreateContext(ICurrentUserContext currentUser)
        {
            var options = new DbContextOptionsBuilder<QMSteelDbContext>().UseSqlite(_connection).Options;
            return new QMSteelDbContext(options, currentUser, TimeProvider.System);
        }

        public void Dispose() => _connection.Dispose();
    }
}
