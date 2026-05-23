using Microsoft.Extensions.DependencyInjection;
using todoApp.Core.Events;
using todoApp.Core.Interfaces;
using todoApp.Core.Services;
using todoApp.Data;
using todoApp.Data.Repositories;
using todoApp.Data.Handlers;
namespace todoApp.DI;

public static class Configuration
{
  public static IServiceCollection ConfigureServces(this IServiceCollection services)
  {
    services.AddScoped<AppDbContext>();
    services.AddSingleton<IPasswordService, PasswordService>();
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<ITodoRepository, TodoRepository>();
    services.AddScoped<IJwtService, JwtService>();
    services.AddTransient<IEventHandler<TaskCompletedEvent>, Data.Handlers.EventHandler>();
    services.AddTransient<IEventHandler<TaskCreatedEvent>, Data.Handlers.EventHandler>();
    services.AddScoped<IEventHandler<TaskDeletedEvent>, Data.Handlers.EventHandler>();
    services.AddScoped<ICachedStatsService, CachedStatsService>();
    return services;
  }
}