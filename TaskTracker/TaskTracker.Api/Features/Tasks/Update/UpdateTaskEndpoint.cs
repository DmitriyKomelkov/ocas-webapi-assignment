using FastEndpoints;
using TaskTracker.Application.Tasks;
using TaskTracker.Application.Tasks.Update;

namespace TaskTracker.Api.Features.Tasks.Update;

internal sealed class UpdateTaskEndpoint : Endpoint<UpdateTaskRequest, TaskDto>
{
    private readonly UpdateTaskHandler _handler;

    public UpdateTaskEndpoint(UpdateTaskHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Put("/tasks/{id}");
        AllowAnonymous();
        Description(b => b.WithTags("Tasks"));
    }

    public override async Task HandleAsync(UpdateTaskRequest req, CancellationToken ct)
    {
        var dto = await _handler.HandleAsync(req.ToCommand(), ct);
        if (dto is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(dto, ct);
    }
}
