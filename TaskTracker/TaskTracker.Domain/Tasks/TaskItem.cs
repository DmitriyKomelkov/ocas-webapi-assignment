using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Tasks;

public sealed class TaskItem
{
    public static readonly int MaxTitleLength = 100;
    public static readonly int MaxDescriptionLength = 2000;

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
        SetDescription(description);
        DueDate = dueDate;
        Status = status;
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
        SetDescription(description);
        DueDate = dueDate;
        Status = status;
    }

    public void ChangeStatus(TaskItemStatus newStatus)
    {
        Status = newStatus;
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

    private void SetDescription(string? description)
    {
        if (description is { Length: var len } && len > MaxDescriptionLength)
        {
            throw new DomainException($"Task description must be {MaxDescriptionLength} characters or fewer.");
        }

        Description = description;
    }
}
