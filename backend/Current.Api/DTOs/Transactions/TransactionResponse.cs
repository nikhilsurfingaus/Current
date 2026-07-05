using Current.Api.Common.Enums;

namespace Current.Api.DTOs.Transactions;

public class TransactionResponse
{
    public Guid Id { get; set; }

    public Guid FromAccountId { get; set; }

    public Guid ToAccountId { get; set; }

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public TransactionCategory Category { get; set; }

    public string? Merchant { get; set; }

    public string? Reference { get; set; }

    public TransactionStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public IReadOnlyList<LedgerEntryResponse> LedgerEntries { get; set; } = Array.Empty<LedgerEntryResponse>();
}
