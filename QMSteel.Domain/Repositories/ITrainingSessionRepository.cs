using QMSteel.Domain.Entities.Training;

namespace QMSteel.Domain.Repositories;

public interface ITrainingSessionRepository : IRepository<TrainingSession>
{
    Task<IReadOnlyList<TrainingSession>> GetBetweenAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<IReadOnlyList<TrainingSession>> GetByEmployeeNumberAsync(string employeeNumber, CancellationToken ct = default);
}
