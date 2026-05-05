namespace TaskTracker.Application.Tasks.Update;

public sealed class UpdateTaskHandler
{
    private readonly ITaskRepository _repository;

    public UpdateTaskHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<TaskDto?> HandleAsync(UpdateTaskCommand command, CancellationToken ct)
    {
        var task = await _repository.GetByIdAsync(command.Id, ct);
        if (task is null)
        {
            return null;
        }

        task.Update(
            title: command.Title,
            description: command.Description,
            dueDate: command.DueDate,
            status: command.Status);

        await _repository.SaveChangesAsync(ct);

        return TaskDto.FromEntity(task);
    }
}
