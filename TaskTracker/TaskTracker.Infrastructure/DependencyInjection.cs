using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Application.Tasks;
using TaskTracker.Infrastructure.Persistence;
using TaskTracker.Infrastructure.Persistence.Repositories;

namespace TaskTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TaskTracker")
            ?? "Data Source=tasktracker.db";

        services.AddDbContext<TaskTrackerDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<ITaskRepository, EfTaskRepository>();

        return services;
    }
}
