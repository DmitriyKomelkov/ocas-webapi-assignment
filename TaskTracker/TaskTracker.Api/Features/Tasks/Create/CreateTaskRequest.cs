using TaskTracker.Application.Tasks.Create;
using TaskTracker.Domain.Tasks;

namespace TaskTracker.Api.Features.Tasks.Create;

public sealed class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public DateTimeOffset? DueDate { get; set; }

    public CreateTaskCommand ToCommand() =>
        new(Title, Description, Status, DueDate);
}
