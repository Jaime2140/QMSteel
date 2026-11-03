using QMSteel.Domain.Execution;

namespace QMSteel.Domain.ValueObjects
{
    // Firma electrónica: quién firmó, cuándo y con qué significado (elaboró, revisó, aprobó, inspeccionó...).
    public sealed record ElectronicSignature
    {
        public Guid UserId { get; }
        public string SignerName { get; }
        public string Meaning { get; }
        public DateTime SignedAtUtc { get; }

        private ElectronicSignature() { SignerName = default!; Meaning = default!; }

        public ElectronicSignature(Guid userId, string signerName, string meaning, DateTime signedAtUtc)
        {
            if (userId == Guid.Empty)
                throw new DomainException("SIGNATURE_USER_REQUIRED", "La firma requiere un usuario.");
            if (string.IsNullOrWhiteSpace(signerName))
                throw new DomainException("SIGNATURE_NAME_REQUIRED", "La firma requiere el nombre del firmante.");
            if (string.IsNullOrWhiteSpace(meaning))
                throw new DomainException("SIGNATURE_MEANING_REQUIRED", "La firma requiere su significado.");

            UserId = userId;
            SignerName = signerName.Trim();
            Meaning = meaning.Trim();
            SignedAtUtc = signedAtUtc;
        }
    }
}
