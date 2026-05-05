using TaskTracker.Domain.Tasks;

namespace TaskTracker.Application.Tasks;

public sealed record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    DateTimeOffset? DueDate)
{
    public static TaskDto FromEntity(TaskItem entity) =>
        new(entity.Id, entity.Title, entity.Description, entity.Status, entity.DueDate);
}
