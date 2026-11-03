using QMSteel.Application.Common;
using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Procedures;
using QMSteel.Domain.Welding;

namespace QMSteel.Application.Procedures
{
    public sealed record EssentialVariablesDto(WeldingProcess Process, string BaseMetalSpecification, string BaseMetalGroup,
        string FillerClassification, string? ShieldingGas, IReadOnlyList<string> Positions, RangeDto ThicknessMm,
        decimal MinPreheatCelsius, RangeDto Amperage, RangeDto Voltage, RangeDto TravelSpeedMmPerMin)
    {
        public static EssentialVariablesDto From(EssentialVariables v) => new(v.Process, v.BaseMetalSpecification, v.BaseMetalGroup,
            v.FillerClassification, v.ShieldingGas, v.Positions.ToList(), RangeDto.From(v.ThicknessMm), v.MinPreheatCelsius,
            RangeDto.From(v.Amperage), RangeDto.From(v.Voltage), RangeDto.From(v.TravelSpeedMmPerMin));

        public EssentialVariables ToDomain() => new(Process, BaseMetalSpecification, BaseMetalGroup, FillerClassification, ShieldingGas,
            Positions, ThicknessMm.ToDomain(), MinPreheatCelsius, Amperage.ToDomain(), Voltage.ToDomain(), TravelSpeedMmPerMin.ToDomain());
    }

    public sealed record CreatePqrRequest(string Number, EssentialVariablesDto Variables, DateOnly TestedOn,
        string? TestingLaboratory = null, Guid? FileId = null);

    public sealed record CreateWpsRequest(string Number, JointType JointType, WeldType WeldType, EssentialVariablesDto Variables,
        bool IsPrequalified, Guid? FileId = null);

    public sealed record UpdateWpsRequest(JointType JointType, WeldType WeldType, EssentialVariablesDto Variables, bool IsPrequalified);

    public sealed record DiscrepancyDto(string Variable, string WpsValue, string PqrValue)
    {
        public static DiscrepancyDto From(VariableDiscrepancy d) => new(d.Variable, d.WpsValue, d.PqrValue);
    }

    public sealed record PqrDto(Guid Id, string Number, EssentialVariablesDto Variables, DateOnly TestedOn, string? TestingLaboratory,
        Guid? FileId, PqrStatus Status, SignatureDto? ApprovedBy)
    {
        public static PqrDto From(Pqr p) => new(p.Id, p.Number, EssentialVariablesDto.From(p.Variables), p.TestedOn,
            p.TestingLaboratory, p.FileId, p.Status, SignatureDto.From(p.ApprovedBy));
    }

    public sealed record WpsDto(Guid Id, string Number, int Revision, string Label, JointType JointType, WeldType WeldType,
        EssentialVariablesDto Variables, bool IsPrequalified, Guid? FileId, WpsStatus Status, ResourceStatus ResourceStatus,
        EligibilityDto Eligibility, SignatureDto? ApprovedBy, DateOnly? ApprovedOn, IReadOnlyList<Guid> SupportingPqrIds)
    {
        public static WpsDto From(Wps w) => new(w.Id, w.Number, w.Revision, w.Label, w.JointType, w.WeldType,
            EssentialVariablesDto.From(w.Variables), w.IsPrequalified, w.FileId, w.Status, w.GetStatus(),
            EligibilityDto.From(w.CheckEligibility()), SignatureDto.From(w.ApprovedBy), w.ApprovedOn, w.SupportingPqrIds.ToList());
    }

    // Renglón de la matriz WPS & PQR con el resultado de la validación de variables esenciales.
    public sealed record WpsMatrixRowDto(Guid WpsId, string WpsLabel, WeldingProcess Process, JointType JointType, RangeDto ThicknessMm,
        WpsStatus Status, bool IsPrequalified, IReadOnlyList<string> PqrNumbers, IReadOnlyList<DiscrepancyDto> Discrepancies);
}
