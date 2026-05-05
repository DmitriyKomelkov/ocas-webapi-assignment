namespace TaskTracker.Application.Tasks.List;

public sealed class ListTasksHandler
{
    private readonly ITaskRepository _repository;

    public ListTasksHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<TaskDto>> HandleAsync(CancellationToken ct)
    {
        var items = await _repository.ListAsync(ct);
        return items.Select(TaskDto.FromEntity).ToList();
    }
}
