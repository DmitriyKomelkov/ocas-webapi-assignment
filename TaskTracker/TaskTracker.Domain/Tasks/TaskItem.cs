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

    public static TaskItem Create(
        string title,
        string? description = null,
        TaskItemStatus status = TaskItemStatus.Todo,
        DateTimeOffset? dueDate = null)
    {
        EnsureValid(title, description, status);

        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Status = status,
            DueDate = dueDate,
        };
    }

    public void Update(
        string title,
        string? description,
        DateTimeOffset? dueDate,
        TaskItemStatus status)
    {
        EnsureValid(title, description, status);

        Title = title;
        Description = description;
        DueDate = dueDate;
        Status = status;
    }

    public void ChangeStatus(TaskItemStatus newStatus)
    {
        EnsureCanTransitionTo(newStatus, currentTitle: Title);
        Status = newStatus;
    }

    private static void EnsureValid(string title, string? description, TaskItemStatus status)
    {
        EnsureValidTitle(title);
        EnsureValidDescription(description);
        EnsureCanTransitionTo(status, currentTitle: title);
    }

    private static void EnsureValidTitle(string title)
    {
        if (title is null)
        {
            throw new DomainException("Task title is required.");
        }

        if (title.Length > MaxTitleLength)
        {
            throw new DomainException($"Task title must be {MaxTitleLength} characters or fewer.");
        }
    }

    private static void EnsureValidDescription(string? description)
    {
        if (description is { Length: var len } && len > MaxDescriptionLength)
        {
            throw new DomainException($"Task description must be {MaxDescriptionLength} characters or fewer.");
        }
    }

    private static void EnsureCanTransitionTo(TaskItemStatus status, string currentTitle)
    {
        if (status == TaskItemStatus.Done && string.IsNullOrWhiteSpace(currentTitle))
        {
            throw new DomainException("Cannot mark a task as Done while its title is empty or whitespace.");
        }
    }
}
