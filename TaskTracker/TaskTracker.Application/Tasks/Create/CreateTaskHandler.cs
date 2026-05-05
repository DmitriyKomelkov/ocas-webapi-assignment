using TaskTracker.Domain.Tasks;

namespace TaskTracker.Application.Tasks.Create;

public sealed class CreateTaskHandler
{
    private readonly ITaskRepository _repository;

    public CreateTaskHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<TaskDto> HandleAsync(CreateTaskCommand command, CancellationToken ct)
    {
        var task = TaskItem.Create(
            title: command.Title,
            description: command.Description,
            status: command.Status,
            dueDate: command.DueDate);

        await _repository.AddAsync(task, ct);
        await _repository.SaveChangesAsync(ct);

        return TaskDto.FromEntity(task);
    }
}
