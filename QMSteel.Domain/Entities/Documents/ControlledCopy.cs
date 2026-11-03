using QMSteel.Domain.Entities.Common;

namespace QMSteel.Domain.Entities.Documents
{
    // Copia controlada distribuida a un destinatario interno o externo.
    public class ControlledCopy : Entity
    {
        public Guid RevisionId { get; private set; }
        public string RecipientName { get; private set; } = default!;
        public string? RecipientEmail { get; private set; }
        public bool IsExternal { get; private set; }
        public bool AllowDownload { get; private set; }
        public bool AllowPrint { get; private set; }
        public DateOnly IssuedOn { get; private set; }
        public DateOnly? ExpiresOn { get; private set; }
        public DateOnly? RevokedOn { get; private set; }
        public string? RevocationReason { get; private set; }

        private ControlledCopy() { }

        internal ControlledCopy(Guid revisionId, string recipientName, string? recipientEmail, bool isExternal,
            bool allowDownload, bool allowPrint, DateOnly issuedOn, DateOnly? expiresOn)
        {
            RevisionId = revisionId;
            RecipientName = recipientName;
            RecipientEmail = recipientEmail;
            IsExternal = isExternal;
            AllowDownload = allowDownload;
            AllowPrint = allowPrint;
            IssuedOn = issuedOn;
            ExpiresOn = expiresOn;
        }

        public bool IsActiveOn(DateOnly date) =>
            RevokedOn is null && date >= IssuedOn && (ExpiresOn is null || date <= ExpiresOn);

        internal void Revoke(DateOnly on, string reason)
        {
            if (RevokedOn is not null) return;
            RevokedOn = on;
            RevocationReason = reason;
        }
    }
}
