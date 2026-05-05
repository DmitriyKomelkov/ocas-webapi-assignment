using TaskTracker.Domain.Common;
using TaskTracker.Domain.Tasks;
using Xunit;

namespace TaskTracker.Domain.UnitTests.Tasks;

public class TaskItemTests
{
    [Fact]
    public void Create_with_valid_data_succeeds()
    {
        var task = TaskItem.Create(
            title: "Buy milk",
            description: "2% organic",
            status: TaskItemStatus.Todo,
            dueDate: new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero));

        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal("Buy milk", task.Title);
        Assert.Equal("2% organic", task.Description);
        Assert.Equal(TaskItemStatus.Todo, task.Status);
        Assert.Equal(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero), task.DueDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void Create_with_blank_title_throws(string title)
    {
        Assert.Throws<DomainException>(() => TaskItem.Create(title));
    }

    [Fact]
    public void Create_with_title_longer_than_max_throws()
    {
        var tooLong = new string('a', TaskItem.MaxTitleLength + 1);

        Assert.Throws<DomainException>(() => TaskItem.Create(tooLong));
    }

    [Fact]
    public void Create_with_title_exactly_max_length_succeeds()
    {
        var atLimit = new string('a', TaskItem.MaxTitleLength);

        var task = TaskItem.Create(atLimit);

        Assert.Equal(atLimit, task.Title);
    }

    [Fact]
    public void Update_with_valid_data_replaces_all_fields()
    {
        var task = TaskItem.Create("Old title");

        task.Update(
            title: "New title",
            description: "New description",
            dueDate: new DateTimeOffset(2030, 6, 1, 0, 0, 0, TimeSpan.Zero),
            status: TaskItemStatus.InProgress);

        Assert.Equal("New title", task.Title);
        Assert.Equal("New description", task.Description);
        Assert.Equal(TaskItemStatus.InProgress, task.Status);
        Assert.Equal(new DateTimeOffset(2030, 6, 1, 0, 0, 0, TimeSpan.Zero), task.DueDate);
    }

    [Fact]
    public void Update_clears_optional_fields_when_null()
    {
        var task = TaskItem.Create(
            title: "Title",
            description: "Has description",
            dueDate: DateTimeOffset.UtcNow);

        task.Update(
            title: "Title",
            description: null,
            dueDate: null,
            status: TaskItemStatus.Todo);

        Assert.Null(task.Description);
        Assert.Null(task.DueDate);
    }

    [Fact]
    public void ChangeStatus_to_Done_with_valid_title_succeeds()
    {
        var task = TaskItem.Create("Buy milk");

        task.ChangeStatus(TaskItemStatus.Done);

        Assert.Equal(TaskItemStatus.Done, task.Status);
    }

    [Fact]
    public void Update_with_whitespace_title_throws_and_leaves_task_unchanged()
    {
        var task = TaskItem.Create("Valid");

        Assert.Throws<DomainException>(() =>
            task.Update(
                title: "   ",
                description: null,
                dueDate: null,
                status: TaskItemStatus.Done));

        Assert.Equal("Valid", task.Title);
        Assert.Equal(TaskItemStatus.Todo, task.Status);
    }

    [Fact]
    public void Create_with_description_longer_than_max_throws()
    {
        var tooLong = new string('x', TaskItem.MaxDescriptionLength + 1);

        Assert.Throws<DomainException>(() => TaskItem.Create(title: "ok", description: tooLong));
    }
}
