namespace TaskTracker.Application.Tasks.Delete;

public sealed class DeleteTaskHandler
{
    private readonly ITaskRepository _repository;

    public DeleteTaskHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> HandleAsync(Guid id, CancellationToken ct)
    {
        var task = await _repository.GetByIdAsync(id, ct);
        if (task is null)
        {
            return false;
        }

        _repository.Remove(task);
        await _repository.SaveChangesAsync(ct);
        return true;
    }
}
