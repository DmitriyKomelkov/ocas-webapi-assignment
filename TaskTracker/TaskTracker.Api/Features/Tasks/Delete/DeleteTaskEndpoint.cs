using FastEndpoints;
using TaskTracker.Application.Tasks.Delete;

namespace TaskTracker.Api.Features.Tasks.Delete;

internal sealed class DeleteTaskEndpoint : Endpoint<DeleteTaskRequest>
{
    private readonly DeleteTaskHandler _handler;

    public DeleteTaskEndpoint(DeleteTaskHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Delete("/tasks/{id}");
        AllowAnonymous();
        Description(b => b.WithTags("Tasks"));
    }

    public override async Task HandleAsync(DeleteTaskRequest req, CancellationToken ct)
    {
        var deleted = await _handler.HandleAsync(req.Id, ct);
        if (!deleted)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
