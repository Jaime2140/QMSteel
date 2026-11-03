using QMSteel.Domain.Entities.Training;

namespace QMSteel.Application.Training
{
    public sealed record AddCourseRequest(string Title, int PlannedMonth, bool IsMandatory, string? TargetArea = null);

    public sealed record CreateSessionRequest(string CourseTitle, string Instructor, DateOnly HeldOn, decimal DurationHours,
        decimal? PassingScore = null, Guid? PlannedCourseId = null);

    public sealed record AttendeeRequest(string EmployeeNumber, string FullName);

    public sealed record RecordAttendanceRequest(string EmployeeNumber, bool Attended, decimal? Score = null);

    public sealed record EvaluateEffectivenessRequest(string EmployeeNumber, bool IsEffective, DateOnly? EvaluatedOn = null);

    public sealed record PlannedCourseDto(Guid Id, string Title, string? TargetArea, int PlannedMonth, bool IsMandatory,
        PlannedCourseStatus Status, Guid? SessionId, string? CancellationReason, bool IsOverdue);

    public sealed record TrainingPlanDto(Guid Id, int Year, decimal CompliancePercentage, IReadOnlyList<PlannedCourseDto> Courses)
    {
        public static TrainingPlanDto From(TrainingPlan plan, DateOnly today)
        {
            var overdue = plan.OverdueCourses(today).Select(c => c.Id).ToHashSet();
            return new(plan.Id, plan.Year, plan.CompliancePercentage(), plan.Courses
                .OrderBy(c => c.PlannedMonth)
                .Select(c => new PlannedCourseDto(c.Id, c.Title, c.TargetArea, c.PlannedMonth, c.IsMandatory, c.Status, c.SessionId,
                    c.CancellationReason, overdue.Contains(c.Id)))
                .ToList());
        }
    }

    public sealed record AttendeeDto(Guid Id, string EmployeeNumber, string FullName, bool? Attended, decimal? Score, bool Approved,
        bool? IsEffective, DateOnly? EffectivenessEvaluatedOn, string? CertificateFolio);

    public sealed record TrainingSessionDto(Guid Id, Guid? PlannedCourseId, string CourseTitle, string Instructor, DateOnly HeldOn,
        decimal DurationHours, decimal? PassingScore, Guid? AttendanceListFileId, TrainingSessionStatus Status,
        decimal EffectivenessPercentage, IReadOnlyList<AttendeeDto> Attendees)
    {
        public static TrainingSessionDto From(TrainingSession s) => new(s.Id, s.PlannedCourseId, s.CourseTitle, s.Instructor, s.HeldOn,
            s.DurationHours, s.PassingScore, s.AttendanceListFileId, s.Status, s.EffectivenessPercentage(),
            s.Attendees.OrderBy(a => a.FullName).Select(a => new AttendeeDto(a.Id, a.EmployeeNumber, a.FullName, a.Attended, a.Score,
                s.Approved(a), a.IsEffective, a.EffectivenessEvaluatedOn, a.CertificateFolio)).ToList());
    }
}
