namespace Payroll.Api.StartupExtensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UsePayrollDefaults(this IApplicationBuilder app)
    {
        // TODO: plug in localization, tracing, etc.
        return app;
    }
}
