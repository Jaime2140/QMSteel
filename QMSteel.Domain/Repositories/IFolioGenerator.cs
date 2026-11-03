namespace QMSteel.Domain.Repositories;

// Folios consecutivos por tenant y prefijo (ej. NC-2026-0001, INS-2026-0001).
public interface IFolioGenerator
{
    Task<string> NextAsync(string prefix, int year, CancellationToken ct = default);
}
