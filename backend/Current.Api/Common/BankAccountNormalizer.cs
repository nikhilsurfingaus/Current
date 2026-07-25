using System.Text.RegularExpressions;

namespace Current.Api.Common;

public static partial class BankAccountNormalizer
{
    public static string NormalizeBsb(string bsb)
    {
        var digitsOnly = Regex.Replace(bsb.Trim(), @"\D", string.Empty);

        if (digitsOnly.Length != 6)
        {
            throw new InvalidOperationException("BSB must be 6 digits.");
        }

        return $"{digitsOnly[..3]}-{digitsOnly[3..]}";
    }

    public static string NormalizeAccountNumber(string accountNumber)
    {
        var digitsOnly = Regex.Replace(accountNumber.Trim(), @"\D", string.Empty);

        if (digitsOnly.Length is < 6 or > 9)
        {
            throw new InvalidOperationException("Account number must be 6 to 9 digits.");
        }

        return digitsOnly;
    }

    public static bool TryNormalizeBsb(string? bsb, out string normalizedBsb)
    {
        normalizedBsb = string.Empty;

        if (string.IsNullOrWhiteSpace(bsb))
        {
            return false;
        }

        try
        {
            normalizedBsb = NormalizeBsb(bsb);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public static bool TryNormalizeAccountNumber(string? accountNumber, out string normalizedAccountNumber)
    {
        normalizedAccountNumber = string.Empty;

        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            return false;
        }

        try
        {
            normalizedAccountNumber = NormalizeAccountNumber(accountNumber);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}
