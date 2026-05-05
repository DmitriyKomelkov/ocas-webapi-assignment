using FastEndpoints;
using TaskTracker.Api.Features.Tasks.GetById;
using TaskTracker.Application.Tasks;
using TaskTracker.Application.Tasks.Create;

namespace TaskTracker.Api.Features.Tasks.Create;

internal sealed class CreateTaskEndpoint : Endpoint<CreateTaskRequest, TaskDto>
{
    private readonly CreateTaskHandler _handler;

    public CreateTaskEndpoint(CreateTaskHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/tasks");
        AllowAnonymous();
        Description(b => b.WithTags("Tasks"));
    }

    public override async Task HandleAsync(CreateTaskRequest req, CancellationToken ct)
    {
        var dto = await _handler.HandleAsync(req.ToCommand(), ct);
        await Send.CreatedAtAsync<GetTaskByIdEndpoint>(
            routeValues: new { id = dto.Id },
            responseBody: dto,
            cancellation: ct);
    }
}
