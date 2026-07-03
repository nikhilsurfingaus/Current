using Current.Api.DTOs.Transactions;

namespace Current.Api.Interfaces;

public interface ITransactionService
{
    Task<TransactionResponse> TransferFundsAsync(TransferRequest request, Guid currentUserId);

    Task<IReadOnlyList<TransactionResponse>> GetAllTransactionsAsync(Guid currentUserId);

    Task<TransactionResponse?> GetTransactionByIdAsync(Guid transactionId, Guid currentUserId);
}
