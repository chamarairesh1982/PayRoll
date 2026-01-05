namespace Payroll.Seeder;

public static class SeedModeParser
{
    public static SeedMode? Parse(string[] args)
    {
        var modeValue = ReadOption(args, "--mode");
        if (string.IsNullOrWhiteSpace(modeValue))
        {
            return null;
        }

        return modeValue.Trim().ToLowerInvariant() switch
        {
            "master" => SeedMode.Master,
            "scenarios" => SeedMode.Scenarios,
            "reset" => SeedMode.Reset,
            _ => null
        };
    }

    public static void PrintUsage()
    {
        Console.WriteLine("Usage: dotnet run --project backend/Payroll.Seeder -- --mode <master|scenarios|reset>");
    }

    private static string? ReadOption(string[] args, string optionName)
    {
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg.Equals(optionName, StringComparison.OrdinalIgnoreCase))
            {
                return i + 1 < args.Length ? args[i + 1] : null;
            }

            if (arg.StartsWith(optionName + "=", StringComparison.OrdinalIgnoreCase))
            {
                return arg[(optionName.Length + 1)..];
            }
        }

        return null;
    }
}
