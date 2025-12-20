namespace Payroll.Application.Security;

public static class SensitiveDataMasker
{
    public static string MaskNic(string nicNumber)
    {
        if (string.IsNullOrWhiteSpace(nicNumber))
        {
            return nicNumber;
        }

        return MaskValue(nicNumber.Trim(), 2, 2);
    }

    public static string? MaskBankAccount(string? bankAccountNumber)
    {
        if (string.IsNullOrWhiteSpace(bankAccountNumber))
        {
            return bankAccountNumber;
        }

        return MaskValue(bankAccountNumber.Trim(), 0, 4);
    }

    public static bool IsMasked(string value) => value.Contains('*', StringComparison.Ordinal);

    private static string MaskValue(string value, int visiblePrefix, int visibleSuffix)
    {
        var length = value.Length;
        if (length <= visiblePrefix + visibleSuffix)
        {
            return new string('*', length);
        }

        var maskedLength = length - visiblePrefix - visibleSuffix;
        return string.Concat(
            value[..visiblePrefix],
            new string('*', maskedLength),
            value[^visibleSuffix..]);
    }
}
