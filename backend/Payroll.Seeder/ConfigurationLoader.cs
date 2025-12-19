using Microsoft.Extensions.Configuration;

namespace Payroll.Seeder;

public static class ConfigurationLoader
{
    public static IConfiguration Load()
    {
        var basePath = Directory.GetCurrentDirectory();
        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true);

        AddIfExists(builder, Path.Combine(basePath, "backend", "Payroll.Seeder", "appsettings.json"));
        AddIfExists(builder, Path.Combine(basePath, "backend", "Payroll.Seeder", "appsettings.Development.json"));
        AddIfExists(builder, Path.Combine(basePath, "backend", "Payroll.Api", "appsettings.json"));
        AddIfExists(builder, Path.Combine(basePath, "backend", "Payroll.Api", "appsettings.Development.json"));
        AddIfExists(builder, Path.Combine(basePath, "..", "Payroll.Seeder", "appsettings.json"));
        AddIfExists(builder, Path.Combine(basePath, "..", "Payroll.Api", "appsettings.json"));

        builder.AddEnvironmentVariables();

        return builder.Build();
    }

    private static void AddIfExists(IConfigurationBuilder builder, string path)
    {
        if (File.Exists(path))
        {
            builder.AddJsonFile(path, optional: true, reloadOnChange: false);
        }
    }
}
