using FastEndpoints;
using TaskTracker.Application.Tasks;
using TaskTracker.Application.Tasks.GetById;

namespace TaskTracker.Api.Features.Tasks.GetById;

internal sealed class GetTaskByIdEndpoint : Endpoint<GetTaskByIdRequest, TaskDto>
{
    private readonly GetTaskByIdHandler _handler;

    public GetTaskByIdEndpoint(GetTaskByIdHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/tasks/{id}");
        AllowAnonymous();
        Description(b => b.WithTags("Tasks"));
    }

    public override async Task HandleAsync(GetTaskByIdRequest req, CancellationToken ct)
    {
        var dto = await _handler.HandleAsync(req.Id, ct);
        if (dto is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(dto, ct);
    }
}
