using Payroll.Application.Interfaces;

namespace Payroll.Api.Middleware;

public class CurrentUserMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUserService)
    {
        // TODO: populate the current user service from claims
        await _next(context);
    }
}
