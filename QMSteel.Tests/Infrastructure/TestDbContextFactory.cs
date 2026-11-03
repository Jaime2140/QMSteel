using Microsoft.EntityFrameworkCore;
using QMSteel.Application.Abstractions;
using QMSteel.Infrastructure.Persistence;

namespace QMSteel.Tests.Infrastructure
{
    internal sealed class FakeCurrentUser : ICurrentUserContext
    {
        public Guid? UserId { get; set; } = Guid.NewGuid();
        public Guid? TenantId { get; set; } = Guid.NewGuid();
    }

    internal static class TestDbContextFactory
    {
        // Solo construye el modelo con el proveedor de PostgreSQL; no abre conexión.
        public static QMSteelDbContext Create(ICurrentUserContext? currentUser = null)
        {
            var options = new DbContextOptionsBuilder<QMSteelDbContext>()
                .UseNpgsql("Host=localhost;Database=qmsteel_tests")
                .Options;

            return new QMSteelDbContext(options, currentUser ?? new FakeCurrentUser(), TimeProvider.System);
        }
    }
}
