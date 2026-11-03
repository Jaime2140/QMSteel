using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Execution;
using QMSteel.Domain.ValueObjects;

namespace QMSteel.Domain.Entities.Procedures
{
    public enum WpsStatus { Draft, InReview, Approved, Obsolete }

    public enum JointType { Butt, Corner, TJoint, Lap, Edge }

    public enum WeldType { CompleteJointPenetration, PartialJointPenetration, Fillet, Plug, Slot }

    public class Wps : AggregateRoot
    {
        public string Number { get; private set; } = default!;
        public int Revision { get; private set; }
        public string Label => $"{Number} Rev. {Revision:00}";
        public JointType JointType { get; private set; }
        public WeldType WeldType { get; private set; }
        public EssentialVariables Variables { get; private set; } = default!;
        public bool IsPrequalified { get; private set; }
        public Guid? FileId { get; private set; }
        public WpsStatus Status { get; private set; } = WpsStatus.Draft;
        public ElectronicSignature? ApprovedBy { get; private set; }
        public DateOnly? ApprovedOn { get; private set; }

        private readonly List<Guid> _supportingPqrIds = new();
        public IReadOnlyCollection<Guid> SupportingPqrIds => _supportingPqrIds;

        private Wps() { }

        public Wps(Guid tenantId, string number, JointType jointType, WeldType weldType, EssentialVariables variables, bool isPrequalified)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new DomainException("WPS_NUMBER_REQUIRED", "El número de WPS es obligatorio.");

            TenantId = tenantId;
            Number = number.Trim().ToUpperInvariant();
            JointType = jointType;
            WeldType = weldType;
            Variables = variables;
            IsPrequalified = isPrequalified;
        }

        public void Update(JointType jointType, WeldType weldType, EssentialVariables variables, bool isPrequalified)
        {
            EnsureDraft();
            JointType = jointType;
            WeldType = weldType;
            Variables = variables;
            IsPrequalified = isPrequalified;
        }

        public void AttachFile(Guid fileId)
        {
            EnsureDraft();
            FileId = fileId;
        }

        public void LinkPqr(Guid pqrId)
        {
            EnsureDraft();
            if (!_supportingPqrIds.Contains(pqrId))
                _supportingPqrIds.Add(pqrId);
        }

        public void UnlinkPqr(Guid pqrId)
        {
            EnsureDraft();
            _supportingPqrIds.Remove(pqrId);
        }

        public void SubmitForReview()
        {
            EnsureDraft();
            if (FileId is null)
                throw new DomainException("WPS_FILE_REQUIRED", "Adjunta el WPS firmado antes de enviarlo a revisión.");
            Status = WpsStatus.InReview;
        }

        public void ReturnToDraft()
        {
            if (Status != WpsStatus.InReview)
                throw new DomainException("WPS_NOT_IN_REVIEW", "El WPS no está en revisión.");
            Status = WpsStatus.Draft;
        }

        // Un WPS calificado por prueba solo se aprueba si sus PQR respaldan todas las variables esenciales.
        public IReadOnlyList<VariableDiscrepancy> Approve(ElectronicSignature approvedBy, DateOnly approvedOn, IEnumerable<Pqr> supportingPqrs)
        {
            if (Status != WpsStatus.InReview)
                throw new DomainException("WPS_NOT_IN_REVIEW", "Solo se aprueba un WPS en revisión.");

            if (!IsPrequalified)
            {
                var linked = supportingPqrs.Where(p => _supportingPqrIds.Contains(p.Id)).ToList();
                if (linked.Count == 0)
                    throw new DomainException("WPS_PQR_REQUIRED", "Un WPS no precalificado requiere al menos un PQR vinculado.");

                var discrepancies = WpsPqrValidator.Validate(Variables, linked);
                if (discrepancies.Count > 0)
                    return discrepancies;
            }

            Status = WpsStatus.Approved;
            ApprovedBy = approvedBy;
            ApprovedOn = approvedOn;
            return Array.Empty<VariableDiscrepancy>();
        }

        // Nueva revisión: el WPS vuelve a borrador y queda bloqueado para uso hasta su reaprobación.
        public void StartNewRevision()
        {
            if (Status != WpsStatus.Approved)
                throw new DomainException("WPS_NOT_APPROVED", "Solo se revisa un WPS aprobado.");

            Revision++;
            Status = WpsStatus.Draft;
            FileId = null;
            ApprovedBy = null;
            ApprovedOn = null;
        }

        public void MakeObsolete() => Status = WpsStatus.Obsolete;

        public ResourceStatus GetStatus() => Status switch
        {
            WpsStatus.Approved => ResourceStatus.Valid,
            WpsStatus.Obsolete => ResourceStatus.Obsolete,
            _ => ResourceStatus.InReview
        };

        // Aplica tanto para seleccionarlo en inspecciones como para imprimirlo.
        public EligibilityResult CheckEligibility() => Status switch
        {
            WpsStatus.Approved => EligibilityResult.Ok(),
            WpsStatus.Obsolete => EligibilityResult.Blocked("WPS_OBSOLETE", $"El WPS {Label} está obsoleto."),
            _ => EligibilityResult.Blocked("WPS_IN_REVIEW", $"El WPS {Label} está en revisión.")
        };

        private void EnsureDraft()
        {
            if (Status != WpsStatus.Draft)
                throw new DomainException("WPS_NOT_EDITABLE", "Solo se modifica un WPS en borrador.");
        }
    }
}
