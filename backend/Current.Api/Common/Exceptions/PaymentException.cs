using Current.Api.Common.Enums;

namespace Current.Api.Common.Exceptions;

public class PaymentException : Exception
{
    public PaymentException(PaymentErrorCode code, string message)
        : base(message)
    {
        Code = code;
    }

    public PaymentErrorCode Code { get; }
}
