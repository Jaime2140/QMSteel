using QMSteel.Application.Abstractions;
using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Audits;
using QMSteel.Domain.Entities.NonConformities;
using QMSteel.Domain.Entities.Receiving;
using QMSteel.Domain.Repositories;

namespace QMSteel.Application.NonConformities
{
    // Alta de NC que dispara el sistema; queda reportada por el usuario cuya acción la originó.
    internal sealed class NonConformityIntake
    {
        private readonly INonConformityRepository _nonConformities;
        private readonly IFolioGenerator _folios;
        private readonly ICurrentUserContext _currentUser;
        private readonly TimeProvider _time;

        public NonConformityIntake(INonConformityRepository nonConformities, IFolioGenerator folios, ICurrentUserContext currentUser,
            TimeProvider time)
        {
            _nonConformities = nonConformities;
            _folios = folios;
            _currentUser = currentUser;
            _time = time;
        }

        public async Task<NonConformity> OpenAsync(NonConformitySource source, Criticality criticality, DefectType defectType,
            string description, Guid sourceReferenceId, Guid? supplierId, CancellationToken ct)
        {
            var tenantId = _currentUser.TenantId ?? throw new InvalidOperationException("La petición no tiene tenant.");
            var userId = _currentUser.UserId ?? throw new InvalidOperationException("La petición no tiene usuario.");
            var today = _time.Today();

            var folio = await _folios.NextAsync(NonConformityFolio.Prefix, today.Year, ct);
            var nonConformity = new NonConformity(tenantId, folio, source, criticality, defectType, description, today, userId,
                _time.UtcNow(), sourceReferenceId, supplierId);

            await _nonConformities.AddAsync(nonConformity, ct);
            return nonConformity;
        }
    }

    // RF-8: un hallazgo de No Conformidad Mayor/Menor abre la NC y queda ligado a ella.
    internal sealed class AuditNonConformityRaisedHandler : IDomainEventHandler<AuditNonConformityRaised>
    {
        private readonly NonConformityIntake _intake;
        private readonly IAuditRepository _audits;

        public AuditNonConformityRaisedHandler(NonConformityIntake intake, IAuditRepository audits)
        {
            _intake = intake;
            _audits = audits;
        }

        public async Task HandleAsync(AuditNonConformityRaised domainEvent, CancellationToken ct = default)
        {
            var criticality = domainEvent.Classification == FindingClassification.MajorNonConformity ? Criticality.High : Criticality.Medium;
            var nonConformity = await _intake.OpenAsync(NonConformitySource.Audit, criticality, DefectType.Procedure,
                $"Hallazgo de auditoría (cláusula {domainEvent.Clause}): {domainEvent.Description}", domainEvent.AuditId, null, ct);

            var audit = await _audits.GetByIdAsync(domainEvent.AuditId, ct)
                ?? throw new InvalidOperationException("La auditoría que levantó el hallazgo no existe.");
            audit.LinkFindingToNonConformity(domainEvent.FindingId, nonConformity.Id);
            _audits.Update(audit);
        }
    }

    // RF-6: MTR incompleto o material dañado en la recepción abre la NCR hacia compras/proveedor.
    internal sealed class ReceivingNonConformityDetectedHandler : IDomainEventHandler<ReceivingNonConformityDetected>
    {
        private readonly NonConformityIntake _intake;

        public ReceivingNonConformityDetectedHandler(NonConformityIntake intake)
        {
            _intake = intake;
        }

        public Task HandleAsync(ReceivingNonConformityDetected domainEvent, CancellationToken ct = default) =>
            _intake.OpenAsync(NonConformitySource.Receiving, Criticality.Medium, DefectType.Material,
                $"Recepción {domainEvent.Folio}: {string.Join("; ", domainEvent.Deviations)}", domainEvent.ReceivingInspectionId,
                domainEvent.SupplierId, ct);
    }
}
