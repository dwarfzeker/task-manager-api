using System.Security.Claims;
using todoApp.Core.Interfaces;

namespace todoApp.Web.Middleware;

public class LastActivityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;

    public LastActivityMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                using var scope = _scopeFactory.CreateScope();
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var user = await userRepository.GetUserByIdAsync(userId);
                if (user != null)
                {
                    user.LasActivity = DateTime.UtcNow;
                    await userRepository.UpdateAsync(user);
                }
                
            }
        }
        await _next(httpContext);
    }
}