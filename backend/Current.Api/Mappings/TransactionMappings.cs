using Current.Api.DTOs.Transactions;
using Current.Api.Entities;

namespace Current.Api.Mappings;

public static class TransactionMappings
{
    public static TransactionResponse ToResponse(this Transaction transaction)
    {
        return new TransactionResponse
        {
            Id = transaction.Id,
            FromAccountId = transaction.FromAccountId,
            ToAccountId = transaction.ToAccountId,
            Amount = transaction.Amount,
            Description = transaction.Description,
            Status = transaction.Status,
            CreatedAt = transaction.CreatedAt,
            LedgerEntries = transaction.LedgerEntries
                .Select(ledgerEntry => ledgerEntry.ToResponse())
                .ToList()
        };
    }
}
