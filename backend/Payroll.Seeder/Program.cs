using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Payroll.Infrastructure.Persistence;
using Payroll.Seeder;

var mode = SeedModeParser.Parse(args);
if (mode is null)
{
    SeedModeParser.PrintUsage();
    return;
}

var configuration = ConfigurationLoader.Load();
var connectionString = configuration["Database:ConnectionString"]
    ?? configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("Database connection string not configured. Set Database:ConnectionString in appsettings or environment.");
    return;
}

var options = new DbContextOptionsBuilder<PayrollDbContext>()
    .UseSqlServer(connectionString)
    .Options;

using var context = new PayrollDbContext(options);
context.Database.Migrate();

var seeder = new PayrollSeeder(context);

switch (mode)
{
    case SeedMode.Master:
        seeder.SeedMasterData();
        Console.WriteLine("Master data seeding completed.");
        break;
    case SeedMode.Scenarios:
        seeder.SeedScenarioData();
        Console.WriteLine("Scenario data seeding completed.");
        break;
    case SeedMode.Reset:
        if (!string.Equals(Environment.GetEnvironmentVariable("SEEDER_ALLOW_RESET"), "true", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Reset mode is disabled. Set SEEDER_ALLOW_RESET=true to allow scenario data reset.");
            return;
        }

        seeder.ResetAndSeedAll();
        Console.WriteLine("Database reset and reseeding completed.");
        break;
}
