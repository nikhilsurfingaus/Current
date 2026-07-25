namespace Current.Api.Common.Exceptions;

public class EmailNotVerifiedException : Exception
{
    public EmailNotVerifiedException()
        : base("Email address is not verified. Check your inbox for the verification code.")
    {
    }
}
