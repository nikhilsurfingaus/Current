namespace Current.Api.Common;

public static class NotificationFormatting
{
    public static string FormatAmount(decimal amount, string currency)
    {
        return $"{amount:N2} {currency.Trim().ToUpperInvariant()}";
    }
}
