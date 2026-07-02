using Current.Api.Common.Enums;

namespace Current.Api.Entities;

public class LedgerEntry
{
    public Guid Id { get; set; }

    public Guid TransactionId { get; set; }

    public Guid AccountId { get; set; }

    public LedgerEntryType EntryType { get; set; }

    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; }

    public Transaction Transaction { get; set; } = null!;

    public Account Account { get; set; } = null!;
}
