using FastEndpoints;
using TaskTracker.Application.Tasks;
using TaskTracker.Application.Tasks.List;

namespace TaskTracker.Api.Features.Tasks.List;

internal sealed class ListTasksEndpoint : EndpointWithoutRequest<IReadOnlyList<TaskDto>>
{
    private readonly ListTasksHandler _handler;

    public ListTasksEndpoint(ListTasksHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/tasks");
        AllowAnonymous();
        Description(b => b.WithTags("Tasks"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var items = await _handler.HandleAsync(ct);
        await Send.OkAsync(items, ct);
    }
}
