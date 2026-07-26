namespace Current.Api.Common.Constants;

public static class EmailVerificationConstants
{
    public const int CodeLength = 6;

    public const int ExpiryMinutes = 10;

    public const int ResendCooldownSeconds = 10 * 60;
}
