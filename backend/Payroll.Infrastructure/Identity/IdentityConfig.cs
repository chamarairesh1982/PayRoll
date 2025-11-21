using Microsoft.Extensions.DependencyInjection;

namespace Payroll.Infrastructure.Identity;

public static class IdentityConfig
{
    public static IServiceCollection AddIdentityLayer(this IServiceCollection services)
    {
        // TODO: wire up ASP.NET Core Identity
        return services;
    }
}
