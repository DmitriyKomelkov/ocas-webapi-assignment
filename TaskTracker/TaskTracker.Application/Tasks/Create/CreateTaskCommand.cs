using TaskTracker.Domain.Tasks;

namespace TaskTracker.Application.Tasks.Create;

public sealed record CreateTaskCommand(
    string Title,
    string? Description,
    TaskItemStatus Status,
    DateTimeOffset? DueDate);
