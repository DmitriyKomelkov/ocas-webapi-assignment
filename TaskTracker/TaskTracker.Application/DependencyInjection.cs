using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Application.Tasks.Create;
using TaskTracker.Application.Tasks.Delete;
using TaskTracker.Application.Tasks.GetById;
using TaskTracker.Application.Tasks.List;
using TaskTracker.Application.Tasks.Update;

namespace TaskTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateTaskHandler>();
        services.AddScoped<ListTasksHandler>();
        services.AddScoped<GetTaskByIdHandler>();
        services.AddScoped<UpdateTaskHandler>();
        services.AddScoped<DeleteTaskHandler>();

        return services;
    }
}
