namespace Current.Api.Common.Constants;

public static class EmailVerificationConstants
{
    public const int CodeLength = 6;

    public const int ExpiryMinutes = 30;

    public const int ResendCooldownSeconds = 60;
}
