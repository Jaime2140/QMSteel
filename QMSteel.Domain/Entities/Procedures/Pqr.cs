using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Execution;
using QMSteel.Domain.ValueObjects;

namespace QMSteel.Domain.Entities.Procedures
{
    public enum PqrStatus { Draft, Approved, Obsolete }

    // Registro de Calificación del Procedimiento: evidencia de prueba que respalda uno o más WPS.
    public class Pqr : AggregateRoot
    {
        public string Number { get; private set; } = default!;
        public EssentialVariables Variables { get; private set; } = default!;
        public DateOnly TestedOn { get; private set; }
        public string? TestingLaboratory { get; private set; }
        public Guid? FileId { get; private set; }
        public PqrStatus Status { get; private set; } = PqrStatus.Draft;
        public ElectronicSignature? ApprovedBy { get; private set; }

        private Pqr() { }

        public Pqr(Guid tenantId, string number, EssentialVariables variables, DateOnly testedOn, string? testingLaboratory = null, Guid? fileId = null)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new DomainException("PQR_NUMBER_REQUIRED", "El número de PQR es obligatorio.");

            TenantId = tenantId;
            Number = number.Trim().ToUpperInvariant();
            Variables = variables;
            TestedOn = testedOn;
            TestingLaboratory = testingLaboratory?.Trim();
            FileId = fileId;
        }

        public void UpdateVariables(EssentialVariables variables)
        {
            if (Status != PqrStatus.Draft)
                throw new DomainException("PQR_NOT_EDITABLE", "Solo se modifica un PQR en borrador.");
            Variables = variables;
        }

        public void AttachFile(Guid fileId) => FileId = fileId;

        public void Approve(ElectronicSignature approvedBy)
        {
            if (Status != PqrStatus.Draft)
                throw new DomainException("PQR_NOT_DRAFT", "El PQR ya fue aprobado o está obsoleto.");
            if (FileId is null)
                throw new DomainException("PQR_FILE_REQUIRED", "Adjunta el PQR firmado antes de aprobarlo.");

            Status = PqrStatus.Approved;
            ApprovedBy = approvedBy;
        }

        public void MakeObsolete() => Status = PqrStatus.Obsolete;
    }
}
