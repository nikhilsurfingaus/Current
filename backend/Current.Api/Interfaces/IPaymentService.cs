using Current.Api.DTOs.Payments;

namespace Current.Api.Interfaces;

public interface IPaymentService
{
    Task<PaymentReceiptResponse> SendPaymentAsync(
        SendPaymentRequest request,
        Guid currentUserId,
        string idempotencyKey);

    Task<PaymentHistoryItemResponse?> GetPaymentReceiptAsync(Guid transactionId, Guid currentUserId);

    Task<IReadOnlyList<PaymentHistoryItemResponse>> GetSentPaymentsAsync(Guid currentUserId);

    Task<IReadOnlyList<PaymentHistoryItemResponse>> GetReceivedPaymentsAsync(Guid currentUserId);

    Task<IReadOnlyList<PaymentHistoryItemResponse>> GetPaymentHistoryAsync(Guid currentUserId);
}
