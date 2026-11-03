using QMSteel.Domain.Execution;
using QMSteel.Domain.ValueObjects;
using QMSteel.Domain.Welding;

namespace QMSteel.Domain.Entities.Procedures
{
    // Variables esenciales compartidas por WPS y PQR (AWS D1.1).
    public sealed record EssentialVariables
    {
        public WeldingProcess Process { get; }
        public string BaseMetalSpecification { get; }
        public string BaseMetalGroup { get; }
        public string FillerClassification { get; }
        public string? ShieldingGas { get; }
        public IReadOnlyList<string> Positions { get; }
        public NumericRange ThicknessMm { get; }
        public decimal MinPreheatCelsius { get; }
        public NumericRange Amperage { get; }
        public NumericRange Voltage { get; }
        public NumericRange TravelSpeedMmPerMin { get; }

        private EssentialVariables()
        {
            BaseMetalSpecification = default!;
            BaseMetalGroup = default!;
            FillerClassification = default!;
            Positions = default!;
            ThicknessMm = default!;
            Amperage = default!;
            Voltage = default!;
            TravelSpeedMmPerMin = default!;
        }

        public EssentialVariables(WeldingProcess process, string baseMetalSpecification, string baseMetalGroup,
            string fillerClassification, string? shieldingGas, IEnumerable<string> positions, NumericRange thicknessMm,
            decimal minPreheatCelsius, NumericRange amperage, NumericRange voltage, NumericRange travelSpeedMmPerMin)
        {
            if (string.IsNullOrWhiteSpace(baseMetalSpecification))
                throw new DomainException("PROCEDURE_BASE_METAL_REQUIRED", "El metal base es obligatorio.");
            if (string.IsNullOrWhiteSpace(baseMetalGroup))
                throw new DomainException("PROCEDURE_BASE_METAL_GROUP_REQUIRED", "El grupo del metal base es obligatorio.");
            if (string.IsNullOrWhiteSpace(fillerClassification))
                throw new DomainException("PROCEDURE_FILLER_REQUIRED", "La clasificación del material de aporte es obligatoria.");
            if (process is WeldingProcess.GMAW && string.IsNullOrWhiteSpace(shieldingGas))
                throw new DomainException("PROCEDURE_GAS_REQUIRED", "GMAW requiere gas de protección.");

            var positionList = positions.Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim().ToUpperInvariant()).Distinct().ToList();
            if (positionList.Count == 0)
                throw new DomainException("PROCEDURE_POSITIONS_REQUIRED", "Indica al menos una posición.");

            Process = process;
            BaseMetalSpecification = baseMetalSpecification.Trim();
            BaseMetalGroup = baseMetalGroup.Trim().ToUpperInvariant();
            FillerClassification = fillerClassification.Trim().ToUpperInvariant();
            ShieldingGas = string.IsNullOrWhiteSpace(shieldingGas) ? null : shieldingGas.Trim().ToUpperInvariant();
            Positions = positionList;
            ThicknessMm = thicknessMm;
            MinPreheatCelsius = minPreheatCelsius;
            Amperage = amperage;
            Voltage = voltage;
            TravelSpeedMmPerMin = travelSpeedMmPerMin;
        }
    }
}
