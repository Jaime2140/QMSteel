using Microsoft.EntityFrameworkCore;
using QMSteel.Application.Abstractions;
using QMSteel.Domain.Repositories;

namespace QMSteel.Infrastructure.Persistence
{
    // Consecutivo atómico con INSERT ... ON CONFLICT (válido en PostgreSQL y SQLite): dos capturas simultáneas nunca reciben el mismo folio.
    // Se ejecuta fuera de la transacción del guardado, así que un guardado fallido puede dejar un hueco en la numeración.
    internal class FolioGenerator : IFolioGenerator
    {
        private readonly QMSteelDbContext _context;
        private readonly ICurrentUserContext _currentUser;

        public FolioGenerator(QMSteelDbContext context, ICurrentUserContext currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<string> NextAsync(string prefix, int year, CancellationToken ct = default)
        {
            var tenantId = _currentUser.TenantId
                ?? throw new InvalidOperationException("No hay un tenant en la petición para generar el folio.");
            var normalizedPrefix = prefix.Trim().ToUpperInvariant();

            // Los parámetros pasan por el mapeo de tipos de EF, así el Guid se guarda igual que en el resto de las tablas.
            var numbers = await _context.Database
                .SqlQuery<int>($"""
                    INSERT INTO "FolioSequences" ("TenantId", "Prefix", "Year", "LastNumber")
                    VALUES ({tenantId}, {normalizedPrefix}, {year}, 1)
                    ON CONFLICT ("TenantId", "Prefix", "Year")
                    DO UPDATE SET "LastNumber" = "FolioSequences"."LastNumber" + 1
                    RETURNING "LastNumber" AS "Value"
                    """)
                .ToListAsync(ct);

            return Format(normalizedPrefix, year, numbers.Single());
        }

        internal static string Format(string prefix, int year, int number) => $"{prefix}-{year}-{number:0000}";
    }
}
