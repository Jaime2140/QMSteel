using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Receiving;
using QMSteel.Domain.Entities.Suppliers;

namespace QMSteel.Application.Suppliers
{
    public sealed record CreateSupplierRequest(string Name, IReadOnlyList<SupplierCategory> Categories, string? ContactName = null,
        string? ContactEmail = null, string? ContactPhone = null);

    public sealed record UpdateSupplierRequest(string Name, IReadOnlyList<SupplierCategory> Categories, string? ContactName,
        string? ContactEmail, string? ContactPhone);

    public sealed record EvaluateSupplierRequest(EvaluationKind Kind, decimal ScorePercent, string? Notes = null, DateOnly? EvaluatedOn = null);

    public sealed record EvaluationDto(Guid Id, EvaluationKind Kind, DateOnly EvaluatedOn, decimal ScorePercent, string? Notes,
        SignatureDto EvaluatedBy)
    {
        public static EvaluationDto From(SupplierEvaluation e) =>
            new(e.Id, e.Kind, e.EvaluatedOn, e.ScorePercent, e.Notes, SignatureDto.From(e.EvaluatedBy)!);
    }

    public sealed record SupplierDto(Guid Id, string Name, string? ContactName, string? ContactEmail, string? ContactPhone,
        SupplierStatus Status, string? StatusReason, decimal? Score, EligibilityDto Eligibility,
        IReadOnlyList<SupplierCategory> Categories, IReadOnlyList<EvaluationDto> Evaluations)
    {
        public static SupplierDto From(Supplier s) => new(s.Id, s.Name, s.ContactName, s.ContactEmail, s.ContactPhone, s.Status,
            s.StatusReason, s.Score, EligibilityDto.From(s.CheckEligibility()), s.Categories.ToList(),
            s.Evaluations.OrderByDescending(e => e.EvaluatedOn).Select(EvaluationDto.From).ToList());
    }

    public sealed record StartReceivingRequest(Guid SupplierId, string PurchaseOrder, string DeliveryNote, DateOnly? ReceivedOn = null,
        string? InvoiceNumber = null);

    public sealed record AddReceivedItemRequest(MaterialType MaterialType, string Description, decimal Quantity, string Unit,
        string? Specification = null, string? HeatNumber = null, MtrStatus? MtrStatus = null, Guid? MtrFileId = null);

    public sealed record UpdateMtrRequest(MtrStatus Status, Guid? FileId);

    public sealed record VisualInspectionRequest(bool Accepted, IReadOnlyList<ReceivingDefect>? Defects = null, string? Notes = null);

    public sealed record ReceivedItemDto(Guid Id, MaterialType MaterialType, string Description, string? Specification, string? HeatNumber,
        decimal Quantity, string Unit, MtrStatus MtrStatus, Guid? MtrFileId, VisualResult VisualResult,
        IReadOnlyList<ReceivingDefect> Defects, string? Notes, bool IsConforming)
    {
        public static ReceivedItemDto From(ReceivedItem i) => new(i.Id, i.MaterialType, i.Description, i.Specification, i.HeatNumber,
            i.Quantity, i.Unit, i.MtrStatus, i.MtrFileId, i.VisualResult, i.Defects.ToList(), i.Notes, i.IsConforming);
    }

    public sealed record ReceivingDto(Guid Id, string Folio, Guid SupplierId, string PurchaseOrder, string DeliveryNote,
        string? InvoiceNumber, DateOnly ReceivedOn, ReceivingStatus Status, SignatureDto? InspectedBy, IReadOnlyList<ReceivedItemDto> Items)
    {
        public static ReceivingDto From(ReceivingInspection r) => new(r.Id, r.Folio, r.SupplierId, r.PurchaseOrder, r.DeliveryNote,
            r.InvoiceNumber, r.ReceivedOn, r.Status, SignatureDto.From(r.InspectedBy), r.Items.Select(ReceivedItemDto.From).ToList());
    }
}
