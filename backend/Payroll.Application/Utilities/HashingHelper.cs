using System.Security.Cryptography;
using System.Text;

namespace Payroll.Application.Utilities;

public static class HashingHelper
{
    public static string ComputeSha256Hash(string value)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = sha.ComputeHash(bytes);
        var builder = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }
}
