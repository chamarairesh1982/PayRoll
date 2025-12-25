using Payroll.Application.Common;
using Payroll.Infrastructure.Identity;

namespace Payroll.Api.Middleware;

/// <summary>
/// Middleware to resolve the tenant from the "X-Tenant-Id" header.
/// This is the core of our multi-tenant resolution strategy for Phase 1.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private const string TenantIdHeaderName = "X-Tenant-Id";

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // 1. Try to get from Authenticated User Claims (Secure source)
        var tenantIdClaim = context.User?.FindFirst("tenant_id")?.Value;
        
        // 2. Fallback to Header (Dev/Testing or Pre-Auth scenarios if allowed)
        if (string.IsNullOrEmpty(tenantIdClaim))
        {
            tenantIdClaim = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        }

        if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            // Assuming ITenantContext has a SetTenantId method, or TenantContext is the concrete type
            // and we can cast if necessary, but the instruction implies direct call.
            // If ITenantContext does not have SetTenantId, this line would need adjustment
            // to cast to the concrete TenantContext type as in the original code.
            // For now, assuming ITenantContext has SetTenantId for simplicity as per instruction.
            if (tenantContext is TenantContext contextImpl)
            {
                contextImpl.SetTenantId(tenantId);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { Error = "Invalid X-Tenant-Id header format. Expected GUID." });
                return;
            }
        }
        else
        {
            // For now, if no tenant is provided, we could either block or use a default.
            // Requirement says "read from header", so let's enforce it for safety.
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { Error = "X-Tenant-Id header is missing." });
            return;
        }

        await _next(context);
    }
}
