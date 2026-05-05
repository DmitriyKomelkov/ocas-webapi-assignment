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

    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Tasks.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<TaskItem>> ListAsync(CancellationToken ct) =>
        await _db.Tasks.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(TaskItem task, CancellationToken ct)
    {
        await _db.Tasks.AddAsync(task, ct);
    }

    public void Remove(TaskItem task)
    {
        _db.Tasks.Remove(task);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
