using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Payroll.Infrastructure.Persistence;

namespace Payroll.Api.StartupExtensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseMigration");

        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PayrollDbContext>();
            var pendingMigrations = dbContext.Database.GetPendingMigrations();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {MigrationCount} pending migrations...", pendingMigrations.Count());
                dbContext.Database.Migrate();
                logger.LogInformation("Database migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("No database migrations to apply.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations.");
            throw;
        }
    }
}
