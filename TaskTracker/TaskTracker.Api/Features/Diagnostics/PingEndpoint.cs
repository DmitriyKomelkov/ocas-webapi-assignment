using FastEndpoints;

namespace TaskTracker.Api.Features.Diagnostics;

internal sealed class PingEndpoint : EndpointWithoutRequest<string>
{
    public override void Configure()
    {
        Get("/ping");
        AllowAnonymous();
        Description(b => b.WithTags("Diagnostics"));
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        return Send.OkAsync("pong", ct);
    }
}
