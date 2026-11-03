using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Suppliers;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Repositories;

namespace QMSteel.Application.Suppliers
{
    // RF-6: directorio AVL, altas/bajas y evaluaciones de proveedores.
    public sealed class SupplierService : ApplicationService
    {
        private readonly ISupplierRepository _suppliers;

        public SupplierService(UseCaseContext context, ISupplierRepository suppliers) : base(context)
        {
            _suppliers = suppliers;
        }

        public async Task<IReadOnlyList<SupplierDto>> ListAsync(SupplierStatus? status = null, SupplierCategory? category = null,
            CancellationToken ct = default) =>
            (await _suppliers.GetAllAsync(status, category, ct)).Select(SupplierDto.From).ToList();

        public Task<Result<SupplierDto>> GetAsync(Guid id, CancellationToken ct = default) =>
            GetAsync(_suppliers, id, SupplierDto.From, ct);

        public async Task<Result<SupplierDto>> CreateAsync(CreateSupplierRequest request, CancellationToken ct = default)
        {
            if (await _suppliers.ExistsByNameAsync(request.Name, ct))
                return Result.Failure<SupplierDto>(ApplicationErrors.Duplicate("SUPPLIER_NAME_DUPLICATE", $"Ya existe el proveedor {request.Name}."));

            var supplier = new Supplier(TenantId, request.Name, request.Categories, request.ContactName, request.ContactEmail, request.ContactPhone);
            return await AddAsync(_suppliers, supplier, SupplierDto.From, ct);
        }

        public Task<Result<SupplierDto>> UpdateAsync(Guid id, UpdateSupplierRequest request, CancellationToken ct = default) =>
            ChangeAsync(_suppliers, id, s =>
            {
                s.UpdateContact(request.Name, request.ContactName, request.ContactEmail, request.ContactPhone);
                s.SetCategories(request.Categories);
            }, SupplierDto.From, ct);

        public Task<Result<SupplierDto>> EvaluateAsync(Guid id, EvaluateSupplierRequest request, CancellationToken ct = default) =>
            SignAndChangeAsync(_suppliers, id, "Evaluó", false,
                (s, signature) => s.Evaluate(request.Kind, request.EvaluatedOn ?? Today, request.ScorePercent, signature, request.Notes),
                SupplierDto.From, ct);

        public Task<Result<SupplierDto>> SuspendAsync(Guid id, string reason, CancellationToken ct = default) =>
            ChangeAsync(_suppliers, id, s => s.Suspend(reason), SupplierDto.From, ct);

        public Task<Result<SupplierDto>> ReinstateAsync(Guid id, CancellationToken ct = default) =>
            ChangeAsync(_suppliers, id, s => s.Reinstate(), SupplierDto.From, ct);

        public Task<Result<SupplierDto>> RemoveAsync(Guid id, string reason, CancellationToken ct = default) =>
            ChangeAsync(_suppliers, id, s => s.Remove(reason), SupplierDto.From, ct);
    }
}
