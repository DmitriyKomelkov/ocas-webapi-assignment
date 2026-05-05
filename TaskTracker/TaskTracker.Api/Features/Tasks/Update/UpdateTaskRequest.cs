using TaskTracker.Application.Tasks.Update;
using TaskTracker.Domain.Tasks;

namespace TaskTracker.Api.Features.Tasks.Update;

public sealed class UpdateTaskRequest
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; }
    public DateTimeOffset? DueDate { get; set; }

    public UpdateTaskCommand ToCommand() =>
        new(Id, Title, Description, Status, DueDate);
}
