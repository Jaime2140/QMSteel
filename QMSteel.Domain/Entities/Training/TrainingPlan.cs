using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Execution;

namespace QMSteel.Domain.Entities.Training
{
    public enum PlannedCourseStatus { Planned, Completed, Cancelled }

    public class PlannedCourse : Entity
    {
        public string Title { get; private set; } = default!;
        public string? TargetArea { get; private set; }
        public int PlannedMonth { get; private set; }
        public bool IsMandatory { get; private set; }
        public PlannedCourseStatus Status { get; private set; } = PlannedCourseStatus.Planned;
        public Guid? SessionId { get; private set; }
        public string? CancellationReason { get; private set; }

        private PlannedCourse() { }

        internal PlannedCourse(string title, string? targetArea, int plannedMonth, bool isMandatory)
        {
            Title = title;
            TargetArea = targetArea;
            PlannedMonth = plannedMonth;
            IsMandatory = isMandatory;
        }

        internal void Complete(Guid sessionId)
        {
            Status = PlannedCourseStatus.Completed;
            SessionId = sessionId;
        }

        internal void Cancel(string reason)
        {
            Status = PlannedCourseStatus.Cancelled;
            CancellationReason = reason;
        }
    }

    // Plan Anual de Capacitación (PAC).
    public class TrainingPlan : AggregateRoot
    {
        public int Year { get; private set; }

        private readonly List<PlannedCourse> _courses = new();
        public IReadOnlyCollection<PlannedCourse> Courses => _courses;

        private TrainingPlan() { }

        public TrainingPlan(Guid tenantId, int year)
        {
            if (year < 2000)
                throw new DomainException("TRAINING_PLAN_YEAR_INVALID", "El año del plan no es válido.");

            TenantId = tenantId;
            Year = year;
        }

        public PlannedCourse AddCourse(string title, int plannedMonth, bool isMandatory, string? targetArea = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("TRAINING_COURSE_TITLE_REQUIRED", "El nombre del curso es obligatorio.");
            if (plannedMonth is < 1 or > 12)
                throw new DomainException("TRAINING_COURSE_MONTH_INVALID", "El mes planeado debe estar entre 1 y 12.");

            var course = new PlannedCourse(title.Trim(), targetArea?.Trim(), plannedMonth, isMandatory);
            _courses.Add(course);
            return course;
        }

        public void CompleteCourse(Guid courseId, Guid sessionId) => Find(courseId).Complete(sessionId);

        public void CancelCourse(Guid courseId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("TRAINING_CANCEL_REASON_REQUIRED", "Indica el motivo de la cancelación.");
            Find(courseId).Cancel(reason.Trim());
        }

        // % de cumplimiento del PAC: cursos impartidos sobre los no cancelados.
        public decimal CompliancePercentage()
        {
            var applicable = _courses.Count(c => c.Status != PlannedCourseStatus.Cancelled);
            if (applicable == 0) return 0;
            return decimal.Round(100m * _courses.Count(c => c.Status == PlannedCourseStatus.Completed) / applicable, 1);
        }

        public IReadOnlyList<PlannedCourse> OverdueCourses(DateOnly today) => _courses
            .Where(c => c.Status == PlannedCourseStatus.Planned
                && (Year < today.Year || (Year == today.Year && c.PlannedMonth < today.Month)))
            .ToList();

        private PlannedCourse Find(Guid courseId) => _courses.SingleOrDefault(c => c.Id == courseId)
            ?? throw new DomainException("TRAINING_COURSE_NOT_FOUND", "El curso no existe en el plan.");
    }
}
