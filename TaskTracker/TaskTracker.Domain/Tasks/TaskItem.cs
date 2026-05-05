using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Tasks;

public sealed class TaskItem
{
    public const int MaxTitleLength = 100;

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public DateTimeOffset? DueDate { get; private set; }

    private TaskItem() { }

    private TaskItem(Guid id, string title, string? description, TaskItemStatus status, DateTimeOffset? dueDate)
    {
        Id = id;
        SetTitle(title);
        Description = description;
        DueDate = dueDate;
        SetStatus(status);
    }

    public static TaskItem Create(
        string title,
        string? description = null,
        TaskItemStatus status = TaskItemStatus.Todo,
        DateTimeOffset? dueDate = null)
    {
        return new TaskItem(Guid.NewGuid(), title, description, status, dueDate);
    }

    public void Update(
        string title,
        string? description,
        DateTimeOffset? dueDate,
        TaskItemStatus status)
    {
        SetTitle(title);
        Description = description;
        DueDate = dueDate;
        SetStatus(status);
    }

    public void ChangeStatus(TaskItemStatus newStatus)
    {
        SetStatus(newStatus);
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Task title is required.");
        }

        if (title.Length > MaxTitleLength)
        {
            throw new DomainException($"Task title must be {MaxTitleLength} characters or fewer.");
        }

        Title = title;
    }

    private void SetStatus(TaskItemStatus newStatus)
    {
        if (newStatus == TaskItemStatus.Done && string.IsNullOrWhiteSpace(Title))
        {
            throw new DomainException("Cannot mark a task as Done while its title is empty.");
        }

        Status = newStatus;
    }
}
