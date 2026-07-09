using Current.Api.DTOs.Payments;

namespace Current.Api.Interfaces;

public interface IPaymentService
{
    Task<PaymentReceiptResponse> SendPaymentAsync(SendPaymentRequest request, Guid currentUserId);
}
