using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Execution;

namespace QMSteel.Domain.Entities.Training
{
    public enum TrainingSessionStatus { Scheduled, Held, Closed, Cancelled }

    public class TrainingAttendee : Entity
    {
        public string EmployeeNumber { get; private set; } = default!;
        public string FullName { get; private set; } = default!;
        public bool? Attended { get; private set; }
        public decimal? Score { get; private set; }
        public bool? IsEffective { get; private set; }
        public DateOnly? EffectivenessEvaluatedOn { get; private set; }
        public string? CertificateFolio { get; private set; }

        private TrainingAttendee() { }

        internal TrainingAttendee(string employeeNumber, string fullName)
        {
            EmployeeNumber = employeeNumber;
            FullName = fullName;
        }

        internal void RecordAttendance(bool attended, decimal? score)
        {
            Attended = attended;
            Score = attended ? score : null;
        }

        internal void EvaluateEffectiveness(bool isEffective, DateOnly evaluatedOn)
        {
            IsEffective = isEffective;
            EffectivenessEvaluatedOn = evaluatedOn;
        }

        internal void IssueCertificate(string folio) => CertificateFolio = folio;
    }

    // Curso impartido con lista de asistencia, evaluación de efectividad y constancias (DC-3 / diplomas).
    public class TrainingSession : AggregateRoot
    {
        public Guid? PlannedCourseId { get; private set; }
        public string CourseTitle { get; private set; } = default!;
        public string Instructor { get; private set; } = default!;
        public DateOnly HeldOn { get; private set; }
        public decimal DurationHours { get; private set; }
        public decimal? PassingScore { get; private set; }
        public Guid? AttendanceListFileId { get; private set; }
        public TrainingSessionStatus Status { get; private set; } = TrainingSessionStatus.Scheduled;

        private readonly List<TrainingAttendee> _attendees = new();
        public IReadOnlyCollection<TrainingAttendee> Attendees => _attendees;

        private TrainingSession() { }

        public TrainingSession(Guid tenantId, string courseTitle, string instructor, DateOnly heldOn, decimal durationHours,
            decimal? passingScore = null, Guid? plannedCourseId = null)
        {
            if (string.IsNullOrWhiteSpace(courseTitle))
                throw new DomainException("TRAINING_COURSE_TITLE_REQUIRED", "El nombre del curso es obligatorio.");
            if (string.IsNullOrWhiteSpace(instructor))
                throw new DomainException("TRAINING_INSTRUCTOR_REQUIRED", "El instructor es obligatorio.");
            if (durationHours <= 0)
                throw new DomainException("TRAINING_DURATION_INVALID", "La duración debe ser mayor a cero.");

            TenantId = tenantId;
            CourseTitle = courseTitle.Trim();
            Instructor = instructor.Trim();
            HeldOn = heldOn;
            DurationHours = durationHours;
            PassingScore = passingScore;
            PlannedCourseId = plannedCourseId;
        }

        public void AddAttendee(string employeeNumber, string fullName)
        {
            EnsureStatus(TrainingSessionStatus.Scheduled);
            if (string.IsNullOrWhiteSpace(employeeNumber) || string.IsNullOrWhiteSpace(fullName))
                throw new DomainException("TRAINING_ATTENDEE_REQUIRED", "El participante requiere número de empleado y nombre.");
            if (_attendees.Any(a => a.EmployeeNumber == employeeNumber.Trim()))
                throw new DomainException("TRAINING_ATTENDEE_DUPLICATED", "El participante ya está inscrito.");

            _attendees.Add(new TrainingAttendee(employeeNumber.Trim(), fullName.Trim()));
        }

        public void RemoveAttendee(string employeeNumber)
        {
            EnsureStatus(TrainingSessionStatus.Scheduled);
            _attendees.RemoveAll(a => a.EmployeeNumber == employeeNumber);
        }

        public void RecordAttendance(string employeeNumber, bool attended, decimal? score = null)
        {
            if (Status is not (TrainingSessionStatus.Scheduled or TrainingSessionStatus.Held))
                throw new DomainException("TRAINING_SESSION_STATE", "La sesión ya fue cerrada o cancelada.");
            Find(employeeNumber).RecordAttendance(attended, score);
        }

        public void MarkHeld(Guid attendanceListFileId)
        {
            EnsureStatus(TrainingSessionStatus.Scheduled);
            if (_attendees.Count == 0)
                throw new DomainException("TRAINING_NO_ATTENDEES", "La sesión no tiene participantes.");
            if (_attendees.Any(a => a.Attended is null))
                throw new DomainException("TRAINING_ATTENDANCE_PENDING", "Registra la asistencia de todos los participantes.");

            AttendanceListFileId = attendanceListFileId;
            Status = TrainingSessionStatus.Held;
        }

        public void EvaluateEffectiveness(string employeeNumber, bool isEffective, DateOnly evaluatedOn)
        {
            EnsureStatus(TrainingSessionStatus.Held);
            var attendee = Find(employeeNumber);
            if (attendee.Attended != true)
                throw new DomainException("TRAINING_NOT_ATTENDED", "Solo se evalúa a quien asistió.");
            attendee.EvaluateEffectiveness(isEffective, evaluatedOn);
        }

        public bool Approved(TrainingAttendee attendee) =>
            attendee.Attended == true && (PassingScore is null || attendee.Score >= PassingScore);

        public void IssueCertificate(string employeeNumber, string folio)
        {
            if (Status is not (TrainingSessionStatus.Held or TrainingSessionStatus.Closed))
                throw new DomainException("TRAINING_SESSION_STATE", "La sesión aún no se imparte.");
            if (string.IsNullOrWhiteSpace(folio))
                throw new DomainException("TRAINING_FOLIO_REQUIRED", "El folio de la constancia es obligatorio.");

            var attendee = Find(employeeNumber);
            if (!Approved(attendee))
                throw new DomainException("TRAINING_NOT_APPROVED", "El participante no acreditó el curso.");
            attendee.IssueCertificate(folio.Trim());
        }

        public void Close()
        {
            EnsureStatus(TrainingSessionStatus.Held);
            if (_attendees.Any(a => a.Attended == true && a.IsEffective is null))
                throw new DomainException("TRAINING_EFFECTIVENESS_PENDING", "Falta evaluar la efectividad de algunos participantes.");
            Status = TrainingSessionStatus.Closed;
        }

        public void Cancel()
        {
            EnsureStatus(TrainingSessionStatus.Scheduled);
            Status = TrainingSessionStatus.Cancelled;
        }

        public decimal EffectivenessPercentage()
        {
            var evaluated = _attendees.Where(a => a.IsEffective is not null).ToList();
            if (evaluated.Count == 0) return 0;
            return decimal.Round(100m * evaluated.Count(a => a.IsEffective == true) / evaluated.Count, 1);
        }

        private TrainingAttendee Find(string employeeNumber) =>
            _attendees.SingleOrDefault(a => a.EmployeeNumber == employeeNumber.Trim())
            ?? throw new DomainException("TRAINING_ATTENDEE_NOT_FOUND", "El participante no está inscrito.");

        private void EnsureStatus(TrainingSessionStatus expected)
        {
            if (Status != expected)
                throw new DomainException("TRAINING_SESSION_STATE", "La sesión no está en el estado requerido para esta acción.");
        }
    }
}
