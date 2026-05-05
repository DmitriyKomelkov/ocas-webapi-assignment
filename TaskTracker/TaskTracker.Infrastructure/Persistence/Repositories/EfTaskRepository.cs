using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.Tasks;
using TaskTracker.Domain.Tasks;

namespace TaskTracker.Infrastructure.Persistence.Repositories;

internal sealed class EfTaskRepository : ITaskRepository
{
    private readonly TaskTrackerDbContext _db;

    public EfTaskRepository(TaskTrackerDbContext db)
    {
        _db = db;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await _db.Tasks.FindAsync([id], ct);

    public async Task<IReadOnlyList<TaskItem>> ListAsync(CancellationToken ct) =>
        await _db.Tasks.AsNoTracking().ToListAsync(ct);

    public void Add(TaskItem task) => _db.Tasks.Add(task);

    public void Remove(TaskItem task) => _db.Tasks.Remove(task);

    public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
