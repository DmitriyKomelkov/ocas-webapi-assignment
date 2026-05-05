using TaskTracker.Domain.Tasks;

namespace TaskTracker.Application.Tasks.Update;

public sealed record UpdateTaskCommand(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    DateTimeOffset? DueDate);
