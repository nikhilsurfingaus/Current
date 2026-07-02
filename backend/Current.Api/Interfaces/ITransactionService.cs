using Current.Api.DTOs.Transactions;

namespace Current.Api.Interfaces;

public interface ITransactionService
{
    Task<TransactionResponse> TransferFundsAsync(TransferRequest request);

    Task<IReadOnlyList<TransactionResponse>> GetAllTransactionsAsync();

    Task<TransactionResponse?> GetTransactionByIdAsync(Guid transactionId);
}
