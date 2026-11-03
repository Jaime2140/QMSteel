using QMSteel.Domain.Entities.Training;

namespace QMSteel.Domain.Repositories;

public interface ITrainingPlanRepository : IRepository<TrainingPlan>
{
    Task<TrainingPlan?> GetByYearAsync(int year, CancellationToken ct = default);
}
