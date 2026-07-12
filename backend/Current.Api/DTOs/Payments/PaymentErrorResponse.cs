using Current.Api.Common.Enums;

namespace Current.Api.DTOs.Payments;

public class PaymentErrorResponse
{
    public PaymentErrorCode Code { get; set; }

    public required string Message { get; set; }
}
