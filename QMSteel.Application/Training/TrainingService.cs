using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Training;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Repositories;

namespace QMSteel.Application.Training
{
    // RF-5: Plan Anual de Capacitación (PAC), sesiones, asistencia, efectividad y constancias.
    public sealed class TrainingService : ApplicationService
    {
        public const string CertificateFolioPrefix = "CONST";

        private readonly ITrainingPlanRepository _plans;
        private readonly ITrainingSessionRepository _sessions;
        private readonly IFolioGenerator _folios;

        public TrainingService(UseCaseContext context, ITrainingPlanRepository plans, ITrainingSessionRepository sessions,
            IFolioGenerator folios) : base(context)
        {
            _plans = plans;
            _sessions = sessions;
            _folios = folios;
        }

        // ---- Plan anual ----

        public async Task<Result<TrainingPlanDto>> GetPlanAsync(int year, CancellationToken ct = default)
        {
            var plan = await _plans.GetByYearAsync(year, ct);
            return plan is null
                ? Result.Failure<TrainingPlanDto>(new Error("TRAININGPLAN_NOT_FOUND", $"No hay plan de capacitación para {year}."))
                : MapPlan(plan);
        }

        public async Task<Result<TrainingPlanDto>> CreatePlanAsync(int year, CancellationToken ct = default)
        {
            if (await _plans.GetByYearAsync(year, ct) is not null)
                return Result.Failure<TrainingPlanDto>(ApplicationErrors.Duplicate("TRAINING_PLAN_DUPLICATE", $"Ya existe el plan de capacitación {year}."));

            return await AddAsync(_plans, new TrainingPlan(TenantId, year), MapPlan, ct);
        }

        public Task<Result<TrainingPlanDto>> AddCourseAsync(Guid planId, AddCourseRequest request, CancellationToken ct = default) =>
            ChangeAsync(_plans, planId, p => p.AddCourse(request.Title, request.PlannedMonth, request.IsMandatory, request.TargetArea), MapPlan, ct);

        public Task<Result<TrainingPlanDto>> CancelCourseAsync(Guid planId, Guid courseId, string reason, CancellationToken ct = default) =>
            ChangeAsync(_plans, planId, p => p.CancelCourse(courseId, reason), MapPlan, ct);

        // ---- Sesiones ----

        public async Task<IReadOnlyList<TrainingSessionDto>> ListSessionsAsync(DateOnly from, DateOnly to, CancellationToken ct = default) =>
            (await _sessions.GetBetweenAsync(from, to, ct)).Select(TrainingSessionDto.From).ToList();

        // Historial de capacitación de un empleado (inspector, soldador o personal operativo).
        public async Task<IReadOnlyList<TrainingSessionDto>> ListByEmployeeAsync(string employeeNumber, CancellationToken ct = default) =>
            (await _sessions.GetByEmployeeNumberAsync(employeeNumber, ct)).Select(TrainingSessionDto.From).ToList();

        public Task<Result<TrainingSessionDto>> GetSessionAsync(Guid id, CancellationToken ct = default) =>
            GetAsync(_sessions, id, TrainingSessionDto.From, ct);

        public async Task<Result<TrainingSessionDto>> CreateSessionAsync(CreateSessionRequest request, CancellationToken ct = default)
        {
            if (request.PlannedCourseId is { } courseId)
            {
                var plan = await _plans.GetByYearAsync(request.HeldOn.Year, ct);
                if (plan is null || plan.Courses.All(c => c.Id != courseId))
                    return Result.Failure<TrainingSessionDto>(Error.Validation("TRAINING_COURSE_NOT_IN_PLAN",
                        $"El curso no pertenece al plan de capacitación {request.HeldOn.Year}."));
            }

            var session = new TrainingSession(TenantId, request.CourseTitle, request.Instructor, request.HeldOn, request.DurationHours,
                request.PassingScore, request.PlannedCourseId);
            return await AddAsync(_sessions, session, TrainingSessionDto.From, ct);
        }

        public Task<Result<TrainingSessionDto>> AddAttendeeAsync(Guid sessionId, AttendeeRequest request, CancellationToken ct = default) =>
            ChangeAsync(_sessions, sessionId, s => s.AddAttendee(request.EmployeeNumber, request.FullName), TrainingSessionDto.From, ct);

        public Task<Result<TrainingSessionDto>> RemoveAttendeeAsync(Guid sessionId, string employeeNumber, CancellationToken ct = default) =>
            ChangeAsync(_sessions, sessionId, s => s.RemoveAttendee(employeeNumber), TrainingSessionDto.From, ct);

        public Task<Result<TrainingSessionDto>> RecordAttendanceAsync(Guid sessionId, RecordAttendanceRequest request, CancellationToken ct = default) =>
            ChangeAsync(_sessions, sessionId, s => s.RecordAttendance(request.EmployeeNumber, request.Attended, request.Score),
                TrainingSessionDto.From, ct);

        // Se marca impartida con la lista de asistencia firmada.
        public Task<Result<TrainingSessionDto>> MarkHeldAsync(Guid sessionId, Guid attendanceListFileId, CancellationToken ct = default) =>
            ChangeAsync(_sessions, sessionId, s => s.MarkHeld(attendanceListFileId), TrainingSessionDto.From, ct);

        public Task<Result<TrainingSessionDto>> EvaluateEffectivenessAsync(Guid sessionId, EvaluateEffectivenessRequest request,
            CancellationToken ct = default) =>
            ChangeAsync(_sessions, sessionId, s => s.EvaluateEffectiveness(request.EmployeeNumber, request.IsEffective,
                request.EvaluatedOn ?? Today), TrainingSessionDto.From, ct);

        // Constancia interna (DC-3 / diploma) con folio consecutivo.
        public async Task<Result<TrainingSessionDto>> IssueCertificateAsync(Guid sessionId, string employeeNumber, CancellationToken ct = default)
        {
            var session = await _sessions.GetByIdAsync(sessionId, ct);
            if (session is null)
                return NotFound<TrainingSession, TrainingSessionDto>(sessionId);

            var folio = await _folios.NextAsync(CertificateFolioPrefix, Today.Year, ct);
            session.IssueCertificate(employeeNumber, folio);
            _sessions.Update(session);
            await SaveAsync(ct);
            return TrainingSessionDto.From(session);
        }

        // Al cerrar una sesión del PAC se da por cumplido el curso planeado.
        public async Task<Result<TrainingSessionDto>> CloseSessionAsync(Guid sessionId, CancellationToken ct = default)
        {
            var session = await _sessions.GetByIdAsync(sessionId, ct);
            if (session is null)
                return NotFound<TrainingSession, TrainingSessionDto>(sessionId);

            session.Close();
            _sessions.Update(session);

            if (session.PlannedCourseId is { } courseId && await _plans.GetByYearAsync(session.HeldOn.Year, ct) is { } plan)
            {
                plan.CompleteCourse(courseId, session.Id);
                _plans.Update(plan);
            }

            await SaveAsync(ct);
            return TrainingSessionDto.From(session);
        }

        public Task<Result<TrainingSessionDto>> CancelSessionAsync(Guid sessionId, CancellationToken ct = default) =>
            ChangeAsync(_sessions, sessionId, s => s.Cancel(), TrainingSessionDto.From, ct);

        private TrainingPlanDto MapPlan(TrainingPlan plan) => TrainingPlanDto.From(plan, Today);
    }
}
