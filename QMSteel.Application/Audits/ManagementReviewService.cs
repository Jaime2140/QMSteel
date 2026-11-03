using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Audits;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Repositories;

namespace QMSteel.Application.Audits
{
    // RF-8: acta de revisión por la dirección (minuta, análisis de riesgos y decisiones).
    public sealed class ManagementReviewService : ApplicationService
    {
        private readonly IManagementReviewRepository _reviews;

        public ManagementReviewService(UseCaseContext context, IManagementReviewRepository reviews) : base(context)
        {
            _reviews = reviews;
        }

        public async Task<IReadOnlyList<ManagementReviewDto>> ListByYearAsync(int year, CancellationToken ct = default) =>
            (await _reviews.GetByYearAsync(year, ct)).Select(ManagementReviewDto.From).ToList();

        public Task<Result<ManagementReviewDto>> GetAsync(Guid id, CancellationToken ct = default) =>
            GetAsync(_reviews, id, ManagementReviewDto.From, ct);

        public Task<Result<ManagementReviewDto>> CreateAsync(CreateManagementReviewRequest request, CancellationToken ct = default) =>
            AddAsync(_reviews, new ManagementReview(TenantId, request.Title, request.HeldOn, request.Attendees), ManagementReviewDto.From, ct);

        public Task<Result<ManagementReviewDto>> SetAttendeesAsync(Guid id, IReadOnlyList<string> attendees, CancellationToken ct = default) =>
            ChangeAsync(_reviews, id, r => r.SetAttendees(attendees), ManagementReviewDto.From, ct);

        public Task<Result<ManagementReviewDto>> AttachMinutesAsync(Guid id, Guid fileId, CancellationToken ct = default) =>
            ChangeAsync(_reviews, id, r => r.AttachMinutes(fileId), ManagementReviewDto.From, ct);

        public Task<Result<ManagementReviewDto>> AttachRiskAnalysisAsync(Guid id, Guid fileId, CancellationToken ct = default) =>
            ChangeAsync(_reviews, id, r => r.AttachRiskAnalysis(fileId), ManagementReviewDto.From, ct);

        public Task<Result<ManagementReviewDto>> AddDecisionAsync(Guid id, AddDecisionRequest request, CancellationToken ct = default) =>
            ChangeAsync(_reviews, id, r => r.AddDecision(request.Description, request.ResponsibleName, request.DueOn), ManagementReviewDto.From, ct);

        public Task<Result<ManagementReviewDto>> CompleteDecisionAsync(Guid id, Guid decisionId, DateOnly? completedOn = null,
            CancellationToken ct = default) =>
            ChangeAsync(_reviews, id, r => r.CompleteDecision(decisionId, completedOn ?? Today), ManagementReviewDto.From, ct);

        public Task<Result<ManagementReviewDto>> ApproveAsync(Guid id, CancellationToken ct = default) =>
            SignAndChangeAsync(_reviews, id, "Aprobó", true, (r, signature) => r.Approve(signature), ManagementReviewDto.From, ct);
    }
}
