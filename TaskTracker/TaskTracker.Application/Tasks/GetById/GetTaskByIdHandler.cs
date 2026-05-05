namespace TaskTracker.Application.Tasks.GetById;

public sealed class GetTaskByIdHandler
{
    private readonly ITaskRepository _repository;

    public GetTaskByIdHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<TaskDto?> HandleAsync(Guid id, CancellationToken ct)
    {
        var task = await _repository.GetByIdAsync(id, ct);
        return task is null ? null : TaskDto.FromEntity(task);
    }
}
