using QMSteel.Domain.Entities.Common;

namespace QMSteel.Domain.Entities.Equipments
{
    // Calibración externa por laboratorio acreditado (EMA/CENAM).
    public class EquipmentCalibration : Entity
    {
        public DateOnly CalibratedOn { get; private set; }
        public DateOnly DueOn { get; private set; }
        public string LaboratoryName { get; private set; } = default!;
        public string? AccreditationNumber { get; private set; }
        public string CertificateNumber { get; private set; } = default!;
        public Guid? CertificateFileId { get; private set; }
        public Guid? SupplierId { get; private set; }
        public bool Passed { get; private set; }

        private EquipmentCalibration() { }

        internal EquipmentCalibration(DateOnly calibratedOn, DateOnly dueOn, string laboratoryName, string? accreditationNumber,
            string certificateNumber, Guid? certificateFileId, Guid? supplierId, bool passed)
        {
            CalibratedOn = calibratedOn;
            DueOn = dueOn;
            LaboratoryName = laboratoryName;
            AccreditationNumber = accreditationNumber;
            CertificateNumber = certificateNumber;
            CertificateFileId = certificateFileId;
            SupplierId = supplierId;
            Passed = passed;
        }

        internal void AttachCertificate(Guid fileId) => CertificateFileId = fileId;
    }
}
