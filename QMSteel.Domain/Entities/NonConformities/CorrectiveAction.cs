using QMSteel.Domain.Entities.Common;

namespace QMSteel.Domain.Entities.NonConformities
{
    public class CorrectiveAction : Entity
    {
        public ActionType Type { get; private set; }
        public string Description { get; private set; } = default!;
        public Guid? ResponsibleUserId { get; private set; }
        public string ResponsibleName { get; private set; } = default!;
        public DateOnly DueOn { get; private set; }
        public DateOnly? CompletedOn { get; private set; }
        public string? CompletionNotes { get; private set; }
        public Guid? EvidenceFileId { get; private set; }

        public bool IsCompleted => CompletedOn is not null;

        private CorrectiveAction() { }

        internal CorrectiveAction(ActionType type, string description, string responsibleName, Guid? responsibleUserId, DateOnly dueOn)
        {
            Type = type;
            Description = description;
            ResponsibleName = responsibleName;
            ResponsibleUserId = responsibleUserId;
            DueOn = dueOn;
        }

        public bool IsOverdueOn(DateOnly date) => !IsCompleted && date > DueOn;

        internal void Complete(DateOnly completedOn, string? notes, Guid? evidenceFileId)
        {
            CompletedOn = completedOn;
            CompletionNotes = notes;
            EvidenceFileId = evidenceFileId;
        }

        internal void Reschedule(DateOnly dueOn) => DueOn = dueOn;
    }
}
