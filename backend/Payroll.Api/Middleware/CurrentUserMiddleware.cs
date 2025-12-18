using System;
using System.Security.Claims;
using System.Linq;
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
        if (currentUserService is SimpleCurrentUserService simpleCurrentUser)
        {
            var user = context.User;

            var headerUserId = context.Request.Headers["X-User-Id"].FirstOrDefault();
            var headerUserName = context.Request.Headers["X-User-Name"].FirstOrDefault();
            var headerRoles = context.Request.Headers["X-User-Roles"].ToString();

            var claimRoles = user?.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();
            var headerRoleList = string.IsNullOrWhiteSpace(headerRoles)
                ? Enumerable.Empty<string>()
                : headerRoles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            simpleCurrentUser.UserId = user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? headerUserId;
            simpleCurrentUser.UserName = user?.Identity?.Name ?? headerUserName;
            simpleCurrentUser.Roles = claimRoles.Concat(headerRoleList)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        await _next(context);
    }
}
