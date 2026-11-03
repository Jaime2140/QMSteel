using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Compliance;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Repositories;

namespace QMSteel.Application.Compliance
{
    // RF-10: objetivos de calidad y su medición mensual.
    public sealed class QualityObjectiveService : ApplicationService
    {
        private readonly IQualityObjectiveRepository _objectives;

        public QualityObjectiveService(UseCaseContext context, IQualityObjectiveRepository objectives) : base(context)
        {
            _objectives = objectives;
        }

        public async Task<IReadOnlyList<QualityObjectiveDto>> ListByYearAsync(int year, CancellationToken ct = default) =>
            (await _objectives.GetByYearAsync(year, ct)).Select(QualityObjectiveDto.From).ToList();

        public Task<Result<QualityObjectiveDto>> GetAsync(Guid id, CancellationToken ct = default) =>
            GetAsync(_objectives, id, QualityObjectiveDto.From, ct);

        public Task<Result<QualityObjectiveDto>> CreateAsync(CreateObjectiveRequest request, CancellationToken ct = default) =>
            AddAsync(_objectives, new QualityObjective(TenantId, request.Name, request.Indicator, request.Unit, request.Target,
                request.Direction, request.Year, request.ResponsibleName), QualityObjectiveDto.From, ct);

        public Task<Result<QualityObjectiveDto>> ChangeTargetAsync(Guid id, decimal target, CancellationToken ct = default) =>
            ChangeAsync(_objectives, id, o => o.ChangeTarget(target), QualityObjectiveDto.From, ct);

        public Task<Result<QualityObjectiveDto>> RecordMeasurementAsync(Guid id, RecordMeasurementRequest request, CancellationToken ct = default) =>
            ChangeAsync(_objectives, id, o => o.RecordMeasurement(request.Month, request.Value, request.Comment), QualityObjectiveDto.From, ct);
    }
}
