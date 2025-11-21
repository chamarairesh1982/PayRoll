using Payroll.Api.Configuration;
using Payroll.Api.Filters;
using Payroll.Api.Mapping;
using Payroll.Application.Interfaces;
using Payroll.Application.Services;
using Payroll.Infrastructure.Identity;
using Payroll.Infrastructure.Logging;
using Payroll.Infrastructure.Persistence;
using Payroll.Shared;

namespace Payroll.Api.StartupExtensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<DatabaseOptions>(configuration.GetSection("Database"));
        services.Configure<PayrollRulesOptions>(configuration.GetSection("PayrollRules"));

        services.AddAutoMapper(typeof(MappingProfile).Assembly);
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<ILeaveService, LeaveService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<ICurrentUserService, SimpleCurrentUserService>();

        services.AddControllers(options => options.Filters.Add<ApiExceptionFilter>());
        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PayrollDbContext>(options => { /* TODO: configure provider */ });
        services.AddIdentityLayer();
        services.AddStructuredLogging(configuration);
        return services;
    }
}

public class SimpleCurrentUserService : ICurrentUserService
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
}
