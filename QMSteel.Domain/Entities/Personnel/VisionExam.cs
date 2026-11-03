using QMSteel.Domain.Entities.Common;

namespace QMSteel.Domain.Entities.Personnel
{
    // Examen anual de la vista: agudeza cercana (Jaeger J1) y percepción de color (Ishihara).
    public class VisionExam : Entity
    {
        public DateOnly ExamDate { get; private set; }
        public DateOnly ExpiresOn { get; private set; }
        public bool NearVisionPassed { get; private set; }
        public bool ColorPerceptionPassed { get; private set; }
        public string? ExaminerName { get; private set; }
        public Guid? FileId { get; private set; }

        public bool Passed => NearVisionPassed && ColorPerceptionPassed;

        private VisionExam() { }

        internal VisionExam(DateOnly examDate, bool nearVisionPassed, bool colorPerceptionPassed, string? examinerName, Guid? fileId)
        {
            ExamDate = examDate;
            ExpiresOn = examDate.AddYears(1);
            NearVisionPassed = nearVisionPassed;
            ColorPerceptionPassed = colorPerceptionPassed;
            ExaminerName = examinerName;
            FileId = fileId;
        }

        public bool IsValidOn(DateOnly date) => Passed && date >= ExamDate && date <= ExpiresOn;
    }
}
