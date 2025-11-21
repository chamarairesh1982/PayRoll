using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Payroll.Infrastructure.Logging;

public static class SerilogConfig
{
    public static IServiceCollection AddStructuredLogging(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: configure Serilog sinks and enrichers
        return services;
    }
}
