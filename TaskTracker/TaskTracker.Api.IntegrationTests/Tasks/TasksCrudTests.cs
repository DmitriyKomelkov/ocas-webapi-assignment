using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskTracker.Domain.Tasks;
using Xunit;

namespace TaskTracker.Api.IntegrationTests.Tasks;

public class TasksCrudTests : IClassFixture<TaskTrackerWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly TaskTrackerWebApplicationFactory _factory;

    public TasksCrudTests(TaskTrackerWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Full_crud_round_trip_works()
    {
        using var client = _factory.CreateClient();

        // CREATE
        var createPayload = new
        {
            title = "Buy milk",
            description = "2% organic",
            status = "Todo",
            dueDate = (DateTimeOffset?)null,
        };
        var createResponse = await client.PostAsJsonAsync("/tasks", createPayload, JsonOptions);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);
        Assert.Equal("Buy milk", created.Title);
        Assert.Equal(TaskItemStatus.Todo, created.Status);

        // GET BY ID
        var getResponse = await client.GetAsync($"/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);
        Assert.Equal(created.Id, fetched!.Id);

        // LIST
        var listResponse = await client.GetAsync("/tasks");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<List<TaskResponse>>(JsonOptions);
        Assert.Contains(list!, t => t.Id == created.Id);

        // UPDATE (full replace)
        var updatePayload = new
        {
            title = "Buy oat milk",
            description = (string?)null,
            status = "InProgress",
            dueDate = new DateTimeOffset(2030, 6, 1, 0, 0, 0, TimeSpan.Zero),
        };
        var updateResponse = await client.PutAsJsonAsync($"/tasks/{created.Id}", updatePayload, JsonOptions);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);
        Assert.Equal("Buy oat milk", updated!.Title);
        Assert.Null(updated.Description);
        Assert.Equal(TaskItemStatus.InProgress, updated.Status);

        // DELETE
        var deleteResponse = await client.DeleteAsync($"/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDelete = await client.GetAsync($"/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
    }

    [Fact]
    public async Task Get_unknown_id_returns_404()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_unknown_id_returns_404()
    {
        using var client = _factory.CreateClient();

        var payload = new { title = "x", description = (string?)null, status = "Todo", dueDate = (DateTimeOffset?)null };
        var response = await client.PutAsJsonAsync($"/tasks/{Guid.NewGuid()}", payload, JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_unknown_id_returns_404()
    {
        using var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"/tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_with_too_long_title_returns_400_validation_problem_details()
    {
        using var client = _factory.CreateClient();

        var payload = new
        {
            title = new string('a', 101),
            description = (string?)null,
            status = "Todo",
            dueDate = (DateTimeOffset?)null,
        };
        var response = await client.PostAsJsonAsync("/tasks", payload, JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.True(problem.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task Create_with_empty_title_and_status_Done_returns_400_problem_details()
    {
        // FluentValidation lets this through (empty title is allowed by spec; Done is a valid enum).
        // The handler then tries to construct TaskItem, the domain rejects it with DomainException,
        // and DomainExceptionHandler translates that to 400 ProblemDetails.
        using var client = _factory.CreateClient();

        var payload = new
        {
            title = "",
            description = (string?)null,
            status = "Done",
            dueDate = (DateTimeOffset?)null,
        };
        var response = await client.PostAsJsonAsync("/tasks", payload, JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.Equal("Domain rule violated", problem.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Create_with_empty_title_and_status_Todo_succeeds()
    {
        using var client = _factory.CreateClient();

        var payload = new
        {
            title = "",
            description = (string?)null,
            status = "Todo",
            dueDate = (DateTimeOffset?)null,
        };
        var response = await client.PostAsJsonAsync("/tasks", payload, JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private sealed record TaskResponse(
        Guid Id,
        string Title,
        string? Description,
        TaskItemStatus Status,
        DateTimeOffset? DueDate);
}
