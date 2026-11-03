using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Receiving;
using QMSteel.Domain.Entities.Suppliers;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Repositories;

namespace QMSteel.Application.Suppliers
{
    // RF-6: inspección de recibo (OC/remisión, material y colada, MTR, inspección visual).
    // Si al cerrar hay desviaciones, el dominio levanta el evento y se abre la NCR automáticamente.
    public sealed class ReceivingService : ApplicationService
    {
        public const string FolioPrefix = "REC";

        private readonly IReceivingInspectionRepository _receivings;
        private readonly ISupplierRepository _suppliers;
        private readonly IFolioGenerator _folios;

        public ReceivingService(UseCaseContext context, IReceivingInspectionRepository receivings, ISupplierRepository suppliers,
            IFolioGenerator folios) : base(context)
        {
            _receivings = receivings;
            _suppliers = suppliers;
            _folios = folios;
        }

        public Task<Result<ReceivingDto>> GetAsync(Guid id, CancellationToken ct = default) =>
            GetAsync(_receivings, id, ReceivingDto.From, ct);

        public async Task<IReadOnlyList<ReceivingDto>> ListBySupplierAsync(Guid supplierId, CancellationToken ct = default) =>
            (await _receivings.GetBySupplierAsync(supplierId, ct)).Select(ReceivingDto.From).ToList();

        // Trazabilidad por número de colada.
        public async Task<IReadOnlyList<ReceivingDto>> ListByHeatNumberAsync(string heatNumber, CancellationToken ct = default) =>
            (await _receivings.GetByHeatNumberAsync(heatNumber, ct)).Select(ReceivingDto.From).ToList();

        public async Task<Result<ReceivingDto>> StartAsync(StartReceivingRequest request, CancellationToken ct = default)
        {
            var supplier = await _suppliers.GetByIdAsync(request.SupplierId, ct);
            if (supplier is null)
                return NotFound<Supplier, ReceivingDto>(request.SupplierId);

            var receivedOn = request.ReceivedOn ?? Today;
            var folio = await _folios.NextAsync(FolioPrefix, receivedOn.Year, ct);
            var receiving = new ReceivingInspection(TenantId, folio, supplier, request.PurchaseOrder, request.DeliveryNote, receivedOn,
                request.InvoiceNumber);

            return await AddAsync(_receivings, receiving, ReceivingDto.From, ct);
        }

        public Task<Result<ReceivingDto>> AddItemAsync(Guid id, AddReceivedItemRequest request, CancellationToken ct = default) =>
            ChangeAsync(_receivings, id, r => r.AddItem(request.MaterialType, request.Description, request.Quantity, request.Unit,
                request.Specification, request.HeatNumber, request.MtrStatus, request.MtrFileId), ReceivingDto.From, ct);

        public Task<Result<ReceivingDto>> UpdateMtrAsync(Guid id, Guid itemId, UpdateMtrRequest request, CancellationToken ct = default) =>
            ChangeAsync(_receivings, id, r => r.UpdateMtr(itemId, request.Status, request.FileId), ReceivingDto.From, ct);

        public Task<Result<ReceivingDto>> RecordVisualInspectionAsync(Guid id, Guid itemId, VisualInspectionRequest request,
            CancellationToken ct = default) =>
            ChangeAsync(_receivings, id, r => r.RecordVisualInspection(itemId, request.Accepted, request.Defects, request.Notes),
                ReceivingDto.From, ct);

        public Task<Result<ReceivingDto>> CompleteAsync(Guid id, CancellationToken ct = default) =>
            SignAndChangeAsync(_receivings, id, "Inspeccionó", false, (r, signature) => r.Complete(signature), ReceivingDto.From, ct);
    }
}
