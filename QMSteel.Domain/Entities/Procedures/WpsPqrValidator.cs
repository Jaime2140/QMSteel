using QMSteel.Domain.ValueObjects;

namespace QMSteel.Domain.Entities.Procedures
{
    public sealed record VariableDiscrepancy(string Variable, string WpsValue, string PqrValue);

    // Verifica que los PQR respalden cada variable esencial del WPS.
    // Cada variable puede quedar respaldada por cualquiera de los PQR aprobados del mismo proceso.
    public static class WpsPqrValidator
    {
        public static IReadOnlyList<VariableDiscrepancy> Validate(EssentialVariables wps, IEnumerable<Pqr> pqrs)
        {
            var discrepancies = new List<VariableDiscrepancy>();
            var candidates = pqrs
                .Where(p => p.Status == PqrStatus.Approved && p.Variables.Process == wps.Process)
                .Select(p => p.Variables)
                .ToList();

            if (candidates.Count == 0)
            {
                discrepancies.Add(new("Proceso", wps.Process.ToString(), "Sin PQR aprobado del mismo proceso"));
                return discrepancies;
            }

            Check(discrepancies, candidates, "Grupo de metal base", wps.BaseMetalGroup, p => p.BaseMetalGroup,
                p => p.BaseMetalGroup == wps.BaseMetalGroup);
            Check(discrepancies, candidates, "Material de aporte", wps.FillerClassification, p => p.FillerClassification,
                p => p.FillerClassification == wps.FillerClassification);
            Check(discrepancies, candidates, "Gas de protección", wps.ShieldingGas ?? "N/A", p => p.ShieldingGas ?? "N/A",
                p => p.ShieldingGas == wps.ShieldingGas);
            Check(discrepancies, candidates, "Espesor (mm)", wps.ThicknessMm.ToString(), p => p.ThicknessMm.ToString(),
                p => p.ThicknessMm.Contains(wps.ThicknessMm));
            Check(discrepancies, candidates, "Precalentamiento mínimo (°C)", wps.MinPreheatCelsius.ToString(), p => p.MinPreheatCelsius.ToString(),
                p => wps.MinPreheatCelsius >= p.MinPreheatCelsius);
            CheckRange(discrepancies, candidates, "Amperaje (A)", wps.Amperage, p => p.Amperage);
            CheckRange(discrepancies, candidates, "Voltaje (V)", wps.Voltage, p => p.Voltage);
            CheckRange(discrepancies, candidates, "Velocidad de avance (mm/min)", wps.TravelSpeedMmPerMin, p => p.TravelSpeedMmPerMin);

            foreach (var position in wps.Positions)
            {
                Check(discrepancies, candidates, "Posición", position, p => string.Join(", ", p.Positions),
                    p => p.Positions.Contains(position));
            }

            return discrepancies;
        }

        private static void CheckRange(List<VariableDiscrepancy> discrepancies, List<EssentialVariables> candidates,
            string variable, NumericRange wpsRange, Func<EssentialVariables, NumericRange> pqrRange) =>
            Check(discrepancies, candidates, variable, wpsRange.ToString(), p => pqrRange(p).ToString(),
                p => pqrRange(p).Contains(wpsRange));

        private static void Check(List<VariableDiscrepancy> discrepancies, List<EssentialVariables> candidates,
            string variable, string wpsValue, Func<EssentialVariables, string> pqrValue, Func<EssentialVariables, bool> supports)
        {
            if (!candidates.Any(supports))
                discrepancies.Add(new(variable, wpsValue, string.Join(" | ", candidates.Select(pqrValue).Distinct())));
        }
    }
}
