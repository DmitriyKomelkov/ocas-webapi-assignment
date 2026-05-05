using TaskTracker.Domain.Tasks;

namespace TaskTracker.Application.Tasks;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<TaskItem>> ListAsync(CancellationToken ct);
    void Add(TaskItem task);
    void Remove(TaskItem task);
    Task<int> SaveChangesAsync(CancellationToken ct);
}
