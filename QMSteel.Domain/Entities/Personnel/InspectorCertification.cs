using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Welding;

namespace QMSteel.Domain.Entities.Personnel
{
    public enum CertificationScheme { Cwi, Scwi, SntTc1A, Other }

    public enum CertificationLevel { None, I, II, III }

    public class InspectorCertification : Entity
    {
        public CertificationScheme Scheme { get; private set; }
        public NdtMethod Method { get; private set; }
        public CertificationLevel Level { get; private set; }
        public string CertificateNumber { get; private set; } = default!;
        public string IssuingBody { get; private set; } = default!;
        public DateOnly IssuedOn { get; private set; }
        public DateOnly ExpiresOn { get; private set; }
        public Guid? FileId { get; private set; }

        private InspectorCertification() { }

        internal InspectorCertification(CertificationScheme scheme, NdtMethod method, CertificationLevel level, string certificateNumber,
            string issuingBody, DateOnly issuedOn, DateOnly expiresOn, Guid? fileId)
        {
            Scheme = scheme;
            Method = method;
            Level = level;
            CertificateNumber = certificateNumber;
            IssuingBody = issuingBody;
            IssuedOn = issuedOn;
            ExpiresOn = expiresOn;
            FileId = fileId;
        }

        public bool IsValidOn(DateOnly date) => date >= IssuedOn && date <= ExpiresOn;

        // CWI/SCWI firman inspección visual; en SNT-TC-1A el nivel I no puede firmar aceptación.
        public bool AllowsSigning(NdtMethod method) => Method == method && Scheme switch
        {
            CertificationScheme.Cwi or CertificationScheme.Scwi => true,
            _ => Level >= CertificationLevel.II
        };

        public string Describe() => Scheme is CertificationScheme.Cwi or CertificationScheme.Scwi
            ? $"{Scheme.ToString().ToUpperInvariant()} {CertificateNumber}"
            : $"{Method} Nivel {Level} {CertificateNumber}";
    }
}
