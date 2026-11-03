using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Entities.Suppliers;
using QMSteel.Domain.Execution;
using QMSteel.Domain.ValueObjects;

namespace QMSteel.Domain.Entities.Receiving
{
    public enum ReceivingStatus { InProgress, Accepted, Rejected }

    public sealed record ReceivingNonConformityDetected(Guid ReceivingInspectionId, string Folio, Guid SupplierId,
        IReadOnlyList<string> Deviations, DateTime OccurredAtUtc) : IDomainEvent;

    // Inspección de recibo (AISC 207 Cap. 9): OC, remisión, factura, MTR e inspección visual.
    public class ReceivingInspection : AggregateRoot
    {
        public string Folio { get; private set; } = default!;
        public Guid SupplierId { get; private set; }
        public string PurchaseOrder { get; private set; } = default!;
        public string DeliveryNote { get; private set; } = default!;
        public string? InvoiceNumber { get; private set; }
        public DateOnly ReceivedOn { get; private set; }
        public ReceivingStatus Status { get; private set; } = ReceivingStatus.InProgress;
        public ElectronicSignature? InspectedBy { get; private set; }

        private readonly List<ReceivedItem> _items = new();
        public IReadOnlyCollection<ReceivedItem> Items => _items;

        private ReceivingInspection() { }

        public ReceivingInspection(Guid tenantId, string folio, Supplier supplier, string purchaseOrder, string deliveryNote,
            DateOnly receivedOn, string? invoiceNumber = null)
        {
            if (string.IsNullOrWhiteSpace(folio))
                throw new DomainException("RECEIVING_FOLIO_REQUIRED", "El folio es obligatorio.");
            if (string.IsNullOrWhiteSpace(purchaseOrder))
                throw new DomainException("RECEIVING_PO_REQUIRED", "La orden de compra es obligatoria.");
            if (string.IsNullOrWhiteSpace(deliveryNote))
                throw new DomainException("RECEIVING_DELIVERY_NOTE_REQUIRED", "La remisión es obligatoria.");

            var eligibility = supplier.CheckEligibility();
            if (!eligibility.IsEligible)
                throw new DomainException(eligibility.ReasonCode!, eligibility.Reason!);

            TenantId = tenantId;
            Folio = folio.Trim().ToUpperInvariant();
            SupplierId = supplier.Id;
            PurchaseOrder = purchaseOrder.Trim();
            DeliveryNote = deliveryNote.Trim();
            InvoiceNumber = invoiceNumber?.Trim();
            ReceivedOn = receivedOn;
        }

        public ReceivedItem AddItem(MaterialType materialType, string description, decimal quantity, string unit,
            string? specification = null, string? heatNumber = null, MtrStatus? mtrStatus = null, Guid? mtrFileId = null)
        {
            EnsureInProgress();
            if (string.IsNullOrWhiteSpace(description))
                throw new DomainException("RECEIVING_ITEM_DESCRIPTION_REQUIRED", "La descripción del material es obligatoria.");
            if (quantity <= 0)
                throw new DomainException("RECEIVING_ITEM_QUANTITY_INVALID", "La cantidad debe ser mayor a cero.");
            if (string.IsNullOrWhiteSpace(unit))
                throw new DomainException("RECEIVING_ITEM_UNIT_REQUIRED", "La unidad es obligatoria.");
            // El acero exige número de colada para mantener la trazabilidad con su MTR.
            if (materialType == MaterialType.Steel && string.IsNullOrWhiteSpace(heatNumber))
                throw new DomainException("RECEIVING_HEAT_REQUIRED", "El acero requiere número de colada.");

            var status = mtrStatus ?? (RequiresMtr(materialType) ? MtrStatus.Missing : MtrStatus.NotRequired);
            if (status == MtrStatus.NotRequired && RequiresMtr(materialType))
                throw new DomainException("RECEIVING_MTR_REQUIRED", "Este material requiere certificado MTR.");
            if (status is MtrStatus.Complete or MtrStatus.Incomplete && mtrFileId is null)
                throw new DomainException("RECEIVING_MTR_FILE_REQUIRED", "Adjunta el certificado MTR.");

            var item = new ReceivedItem(materialType, description.Trim(), specification?.Trim(), heatNumber?.Trim().ToUpperInvariant(),
                quantity, unit.Trim(), status, mtrFileId);
            _items.Add(item);
            return item;
        }

        public void UpdateMtr(Guid itemId, MtrStatus status, Guid? fileId)
        {
            EnsureInProgress();
            var item = Find(itemId);
            if (status == MtrStatus.NotRequired && RequiresMtr(item.MaterialType))
                throw new DomainException("RECEIVING_MTR_REQUIRED", "Este material requiere certificado MTR.");
            if (status is MtrStatus.Complete or MtrStatus.Incomplete && fileId is null)
                throw new DomainException("RECEIVING_MTR_FILE_REQUIRED", "Adjunta el certificado MTR.");
            item.UpdateMtr(status, fileId);
        }

        public void RecordVisualInspection(Guid itemId, bool accepted, IEnumerable<ReceivingDefect>? defects = null, string? notes = null)
        {
            EnsureInProgress();
            var defectList = (defects ?? Enumerable.Empty<ReceivingDefect>()).ToList();
            if (!accepted && defectList.Count == 0 && string.IsNullOrWhiteSpace(notes))
                throw new DomainException("RECEIVING_DEFECT_REQUIRED", "Indica el defecto encontrado.");
            Find(itemId).RecordVisual(accepted, defectList, notes?.Trim());
        }

        // Cierra la inspección; si hay desviaciones dispara la creación de la NCR hacia Compras/Proveedor.
        public void Complete(ElectronicSignature inspectedBy)
        {
            EnsureInProgress();
            if (_items.Count == 0)
                throw new DomainException("RECEIVING_NO_ITEMS", "La inspección no tiene partidas.");
            if (_items.Any(i => i.VisualResult == VisualResult.Pending))
                throw new DomainException("RECEIVING_VISUAL_PENDING", "Falta la inspección visual de algunas partidas.");

            InspectedBy = inspectedBy;
            var deviations = _items.Where(i => !i.IsConforming).Select(i => i.DescribeDeviation()).ToList();
            Status = deviations.Count == 0 ? ReceivingStatus.Accepted : ReceivingStatus.Rejected;

            if (deviations.Count > 0)
                Raise(new ReceivingNonConformityDetected(Id, Folio, SupplierId, deviations, inspectedBy.SignedAtUtc));
        }

        public static bool RequiresMtr(MaterialType materialType) =>
            materialType is MaterialType.Steel or MaterialType.WeldingConsumable or MaterialType.Fastener;

        private ReceivedItem Find(Guid itemId) => _items.SingleOrDefault(i => i.Id == itemId)
            ?? throw new DomainException("RECEIVING_ITEM_NOT_FOUND", "La partida no existe.");

        private void EnsureInProgress()
        {
            if (Status != ReceivingStatus.InProgress)
                throw new DomainException("RECEIVING_CLOSED", "La inspección de recibo ya fue cerrada.");
        }
    }
}
