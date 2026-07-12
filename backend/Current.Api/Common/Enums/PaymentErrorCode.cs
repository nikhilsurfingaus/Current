namespace Current.Api.Common.Enums;

public enum PaymentErrorCode
{
    InvalidAmount,
    RecipientEmailRequired,
    IdempotencyKeyRequired,
    SourceAccountNotFound,
    RecipientNotFound,
    RecipientAccountNotFound,
    SelfPaymentNotAllowed,
    CurrencyNotSupported,
    InsufficientFunds,
    DuplicatePayment
}
