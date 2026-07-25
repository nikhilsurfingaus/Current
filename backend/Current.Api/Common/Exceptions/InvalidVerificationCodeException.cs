namespace Current.Api.Common.Exceptions;

public class InvalidVerificationCodeException : Exception
{
    public InvalidVerificationCodeException()
        : base("Invalid or expired verification code.")
    {
    }
}
