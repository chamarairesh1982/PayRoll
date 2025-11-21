using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Payroll.Api.Configuration;
using Payroll.Api.Filters;
using Payroll.Api.Mapping;
using Payroll.Application.Interfaces;
using Payroll.Application.Leave;
using Payroll.Application.Overtime;
using Payroll.Application.PayrollConfig;
using Payroll.Application.Services;
using Payroll.Application.Validators.Employees;
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
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateEmployeeRequestDtoValidator>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<ILeaveRequestService, LeaveRequestService>();
        services.AddScoped<IOvertimeService, OvertimeService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAllowanceTypeService, AllowanceTypeService>();
        services.AddScoped<IDeductionTypeService, DeductionTypeService>();
        services.AddScoped<ICurrentUserService, SimpleCurrentUserService>();

        services.AddControllers(options => options.Filters.Add<ApiExceptionFilter>());
        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseOptions = new DatabaseOptions();
        configuration.GetSection("Database").Bind(databaseOptions);
        var connectionString = databaseOptions.ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Database connection string not configured.");
        }

        services.AddDbContext<PayrollDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IPayrollDbContext>(provider => provider.GetRequiredService<PayrollDbContext>());
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
